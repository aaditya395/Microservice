using LeaveService.Models;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using UserService.DatabaseContext;
using RabbitMQ.Client;

namespace LeaveService.Controllers;

[ApiController]
[Route("api/leave")]
public class LeaveController : ControllerBase
{
    private readonly LeaveContext _context;
    private readonly ConnectionFactory _factory;

    public LeaveController(LeaveContext context)
    {
        _context = context;
        _factory = new ConnectionFactory() { HostName = "localhost" };
    }

    [HttpPost("apply")]
    public IActionResult ApplyLeave([FromBody] LeaveRequest request)
    {
        var balance = _context.LeaveBalances.FirstOrDefault(b => b.EmployeeId == request.EmployeeId);
        if (balance == null)
        {
            return BadRequest("Employee leave balance not found.");
        }

        int requestedDays = (request.EndDate - request.StartDate).Days + 1;

        if (request.LeaveType == "Annual" && requestedDays > balance.AnnualLeaveBalance)
        {
            return BadRequest("Insufficient annual leave balance.");
        }
        if (request.LeaveType == "Sick" && requestedDays > balance.SickLeaveBalance)
        {
            return BadRequest("Insufficient sick leave balance.");
        }

        _context.LeaveRequests.Add(request);
        _context.SaveChanges();

        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "leaveQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        string message = JsonSerializer.Serialize(request);
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "", routingKey: "leaveQueue", basicProperties: null, body: body);

        return Ok(new { Message = "Leave request submitted" });
    }

    [HttpPost("approve/{id}")]
    public IActionResult ApproveLeave(int id)
    {
        var leave = _context.LeaveRequests.FirstOrDefault(l => l.Id == id);
        if (leave == null || leave.Status != "Pending")
        {
            return BadRequest("Leave request not found or already processed.");
        }

        leave.Status = "Approved";
        _context.SaveChanges();
        SendNotification(leave, "Your leave request has been approved.");
        return Ok(new { Message = "Leave approved" });
    }

    [HttpPost("reject/{id}")]
    public IActionResult RejectLeave(int id)
    {
        var leave = _context.LeaveRequests.FirstOrDefault(l => l.Id == id);
        if (leave == null || leave.Status != "Pending")
        {
            return BadRequest("Leave request not found or already processed.");
        }

        leave.Status = "Rejected";
        _context.SaveChanges();
        SendNotification(leave, "Your leave request has been rejected.");
        return Ok(new { Message = "Leave rejected" });
    }

    [HttpGet("balances")]
    public IActionResult GetAllLeaveBalances()
    {
        var balances = _context.LeaveBalances.ToList();
        return Ok(balances);
    }

    private void SendNotification(LeaveRequest leave, string message)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "notificationQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var notificationMessage = JsonSerializer.Serialize(new { leave.EmployeeId, Message = message });
        var body = Encoding.UTF8.GetBytes(notificationMessage);

        channel.BasicPublish(exchange: "", routingKey: "notificationQueue", basicProperties: null, body: body);
    }
}
