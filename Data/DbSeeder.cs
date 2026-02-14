using Telemedicine.API.Models;
using BCrypt.Net;

namespace Telemedicine.API.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (context.Users.Any()) return; // DB has been seeded

            var doctor = new User
            {
                FullName = "Dr. Strange",
                Email = "doctor@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = "Doctor"
            };

            var patient = new User
            {
                FullName = "Tony Stark",
                Email = "patient@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = "Patient"
            };

            context.Users.AddRange(doctor, patient);
            context.SaveChanges();

            var appointment = new Appointment
            {
                DoctorId = doctor.Id,
                PatientId = patient.Id,
                ScheduledTime = DateTime.UtcNow.AddHours(1),
                Status = "Scheduled"
            };

            context.Appointments.Add(appointment);
            context.SaveChanges();

            // Seed a Room for easier testing
            var room = new ConsultationRoom
            {
                RoomId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                AppointmentId = appointment.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.ConsultationRooms.Add(room);
            context.SaveChanges();
        }
    }
}
