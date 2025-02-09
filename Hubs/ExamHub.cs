using Microsoft.AspNetCore.SignalR;

namespace StudentHub.Hubs
{
    public class ExamHub : Hub
    {
        public async Task RegisterStudent(long examId)
        {
            await Clients.All.SendAsync("UpdateRegisteredStudents", examId);
        }

        public async Task UnregisterStudent(long examId)
        {
            await Clients.All.SendAsync("UpdateRegisteredStudents", examId);
        }
    }
}
