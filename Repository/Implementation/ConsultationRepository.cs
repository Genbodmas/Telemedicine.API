using System.Data;
using Microsoft.Data.SqlClient;
using Telemedicine.API.Models;
using Telemedicine.API.Models.Requests;
using Telemedicine.API.Repository.Interface;
using Telemedicine.API.DTOs;

namespace Telemedicine.API.Repository.Implementation
{
    public class ConsultationRepository : IConsultationRepository
    {
        private readonly string _connectionString;
        private readonly Services.EncryptionService _encryptionService;

        public ConsultationRepository(IConfiguration configuration, Services.EncryptionService encryptionService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("Connection string not found.");
            _encryptionService = encryptionService;
        }

        public async Task<ApiResponse<Guid>> CreateRoomAsync(CreateRoomRequest request)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_CreateConsultationRoom", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@AppointmentId", request.AppointmentId);
                cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var roomId = reader.GetGuid(0);
                    return ApiResponse<Guid>.Success("Room created successfully.", roomId);
                }

                return ApiResponse<Guid>.Fail("Failed to create room.", 500);
            }
            catch (SqlException ex)
            {
                // Custom error handling based on SP throw (50001)
                if (ex.Number == 50001)
                {
                    return ApiResponse<Guid>.Fail(ex.Message, 403);
                }
                return ApiResponse<Guid>.Fail($"Database error: {ex.Message}", 500);
            }
            catch (Exception ex)
            {
                return ApiResponse<Guid>.Fail($"An error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<bool>> JoinRoomAsync(JoinRoomRequest request)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_JoinRoom", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@RoomId", request.RoomId);
                cmd.Parameters.AddWithValue("@UserId", request.UserId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var result = reader.GetInt32(0); // 1 = Success, 0 = Fail
                    if (result == 1)
                    {
                        return ApiResponse<bool>.Success("Joined room successfully.", true);
                    }
                }
                
                return ApiResponse<bool>.Fail("Unauthorized access to this room.", 403);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"An error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<bool>> AddChatAsync(AddChatRequest request)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_AddChat", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@RoomId", request.RoomId);
                cmd.Parameters.AddWithValue("@SenderId", request.SenderId);
                cmd.Parameters.AddWithValue("@Message", request.Message);
                cmd.Parameters.AddWithValue("@FileUrl", (object)request.FileUrl ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
                return ApiResponse<bool>.Success("Message sent.", true);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50001)
                    return ApiResponse<bool>.Fail("Unauthorized.", 403);

                return ApiResponse<bool>.Fail($"Database error: {ex.Message}", 500);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error sending message: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<bool>> AddNoteAsync(AddNoteRequest request)
        {
             try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_AddDoctorNote", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@AppointmentId", request.AppointmentId);
                cmd.Parameters.AddWithValue("@DoctorId", request.DoctorId);
                cmd.Parameters.AddWithValue("@Content", request.Content);

                await cmd.ExecuteNonQueryAsync();
                return ApiResponse<bool>.Success("Note added successfully.", true);
            }
            catch (SqlException ex)
            {
                 if (ex.Number == 50001)
                    return ApiResponse<bool>.Fail("Unauthorized: Not the assigned doctor.", 403);

                return ApiResponse<bool>.Fail($"Database error: {ex.Message}", 500);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error adding note: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<bool>> EndSessionAsync(EndSessionRequest request)
        {
             try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_EndConsultation", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@RoomId", request.RoomId);
                cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                await cmd.ExecuteNonQueryAsync();
                return ApiResponse<bool>.Success("Session ended.", true);
            }
            catch (SqlException ex)
            {
                 if (ex.Number == 50001)
                    return ApiResponse<bool>.Fail("Unauthorized.", 403);

                return ApiResponse<bool>.Fail($"Database error: {ex.Message}", 500);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error ending session: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<User>>> GetDoctorsAsync()
        {
            try
            {
                var doctors = new List<User>();
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_GetDoctors", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    doctors.Add(new User
                    {
                        Id = reader.GetInt32(0),
                        FullName = reader.GetString(1),
                        Email = reader.GetString(2),
                        Role = "Doctor" // Hardcoded since we queried for Doctors
                    });
                }

                return ApiResponse<IEnumerable<User>>.Success("Doctors retrieved.", doctors);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<User>>.Fail($"Error retrieving doctors: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<int>> BookAppointmentAsync(int patientId, int doctorId, DateTime scheduledTime, string reason)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_BookAppointment", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@PatientId", patientId);
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                cmd.Parameters.AddWithValue("@ScheduledTime", scheduledTime);
                cmd.Parameters.AddWithValue("@Reason", (object)reason ?? DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                if (result != null && int.TryParse(result.ToString(), out int appointmentId))
                {
                    return ApiResponse<int>.Success("Appointment booked successfully.", appointmentId);
                }

                return ApiResponse<int>.Fail("Failed to book appointment.", 500);
            }
            catch (Exception ex)
            {
                return ApiResponse<int>.Fail($"Error booking appointment: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<dynamic>>> GetUserAppointmentsAsync(int userId, string role)
        {
            try
            {
                var appointments = new List<dynamic>();
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_GetUserAppointments", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Role", role);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    appointments.Add(new
                    {
                        AppointmentId = reader.GetInt32(0),
                        ScheduledTime = reader.GetDateTime(1),
                        Status = reader.GetString(2),
                        CounterpartName = reader.GetString(3),
                        RoomId = reader.IsDBNull(4) ? (Guid?)null : reader.GetGuid(4)
                    });
                }

                return ApiResponse<IEnumerable<dynamic>>.Success("Appointments retrieved.", appointments);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<dynamic>>.Fail($"Error retrieving appointments: {ex.Message}", 500);
            }
        }
        public async Task<ApiResponse<IEnumerable<dynamic>>> GetChatHistoryAsync(Guid roomId)
        {
            try
            {
                var history = new List<dynamic>();
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_GetChatHistory", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@RoomId", roomId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    try 
                    {
                        var encryptedMsg = reader.GetString(1);
                        var decryptedMsg = _encryptionService.Decrypt(encryptedMsg);
                        

                        history.Add(new
                        {
                            SenderId = reader.GetInt32(0),
                            Message = decryptedMsg,
                            FileUrl = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Timestamp = reader.GetDateTime(3),
                            SenderName = reader.GetString(4)
                        });
                    }
                    catch
                    {
                         history.Add(new
                        {
                            SenderId = reader.GetInt32(0),
                            Message = reader.GetString(1), // Fallback
                            FileUrl = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Timestamp = reader.GetDateTime(3),
                            SenderName = reader.GetString(4)
                        });
                    }
                }

                return ApiResponse<IEnumerable<dynamic>>.Success("History retrieved.", history);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<dynamic>>.Fail($"Error retrieving history: {ex.Message}", 500);
            }
        }

        // Phase 7: Doctor Availability
        public async Task<ApiResponse<bool>> SetDoctorAvailabilityAsync(int doctorId, SetAvailabilityDto request)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                
                // Parse TimeSpans
                if (!TimeSpan.TryParse(request.StartTime, out TimeSpan start) || 
                    !TimeSpan.TryParse(request.EndTime, out TimeSpan end))
                {
                    return ApiResponse<bool>.Fail("Invalid time format.", 400);
                }

                using var cmd = new SqlCommand("sp_SetDoctorAvailability", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                cmd.Parameters.AddWithValue("@DayOfWeek", request.DayOfWeek);
                cmd.Parameters.AddWithValue("@StartTime", start);
                cmd.Parameters.AddWithValue("@EndTime", end);
                cmd.Parameters.AddWithValue("@IsActive", request.IsActive);
                
                await cmd.ExecuteNonQueryAsync();
                return ApiResponse<bool>.Success("Availability set.", true);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50001) return ApiResponse<bool>.Fail(ex.Message, 403);
                return ApiResponse<bool>.Fail($"Database error: {ex.Message}", 500);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<List<AvailabilitySlotDto>>> GetDoctorAvailabilityAsync(int doctorId)
        {
            try
            {
                var availability = new List<AvailabilitySlotDto>();
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetDoctorAvailability", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    availability.Add(new AvailabilitySlotDto
                    {
                        Id = reader.GetInt32(0),
                        DayOfWeek = reader.GetInt32(1),
                        StartTime = reader.GetString(2), 
                        EndTime = reader.GetString(3),
                        IsActive = reader.GetBoolean(4)
                    });
                }
                return ApiResponse<List<AvailabilitySlotDto>>.Success("Availability retrieved.", availability);
            }
            catch (Exception ex)
            {
                // Fallback if casting fails (e.g. if SP returns string)
                 return ApiResponse<List<AvailabilitySlotDto>>.Fail($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeleteDoctorAvailabilityAsync(int id, int doctorId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_DeleteDoctorAvailability", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && (int)result > 0)
                    return ApiResponse<bool>.Success("Slot deleted.", true);
                return ApiResponse<bool>.Fail("Slot not found.", 404);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error: {ex.Message}", 500);
            }
        }

    public async Task<ApiResponse<bool>> AddRecommendationAsync(int doctorId, Guid roomId, string details)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("sp_AddRecommendationByRoom", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);
            cmd.Parameters.AddWithValue("@RoomId", roomId);
            cmd.Parameters.AddWithValue("@Details", details);
            await cmd.ExecuteNonQueryAsync();
            return ApiResponse<bool>.Success("Recommendation added.", true);
        }
        catch (SqlException ex)
        {
            if (ex.Number == 50001) return ApiResponse<bool>.Fail(ex.Message, 403);
            return ApiResponse<bool>.Fail($"Database error: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
             return ApiResponse<bool>.Fail($"Error: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponse<string>> GetRecommendationAsync(Guid roomId)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("sp_GetRecommendationByRoom", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@RoomId", roomId);
            var result = await cmd.ExecuteScalarAsync();
            
            if (result != null && result != DBNull.Value)
            {
                return ApiResponse<string>.Success("Found", (string)result);
            }
            return ApiResponse<string>.Success("None", null);
        }
        catch (Exception ex)
        {
             return ApiResponse<string>.Fail($"Error: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponse<List<NoteDto>>> GetConsultationNotesAsync(Guid roomId)
    {
        try
        {
            var notes = new List<NoteDto>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("sp_GetConsultationNotes", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@RoomId", roomId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                notes.Add(new NoteDto
                {
                    Id = reader.GetInt32(0),
                    Content = reader.GetString(1),
                    CreatedAt = reader.GetDateTime(2),
                    DoctorName = reader.GetString(3)
                });
            }

            return ApiResponse<List<NoteDto>>.Success("Notes retrieved.", notes);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<NoteDto>>.Fail($"Error fetching notes: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponse<RoomDetailsDto>> GetRoomDetailsAsync(Guid roomId)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("sp_GetRoomDetails", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@RoomId", roomId);
            
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var details = new RoomDetailsDto
                {
                    RoomId = reader.GetGuid(0),
                    AppointmentId = reader.GetInt32(1),
                    ScheduledTime = reader.GetDateTime(2),
                    Status = reader.GetString(3),
                    Reason = reader.IsDBNull(4) ? null : reader.GetString(4),
                    PatientName = reader.GetString(5),
                    DoctorName = reader.GetString(6),
                    PatientId = reader.GetInt32(7),
                    DoctorId = reader.GetInt32(8)
                };
                return ApiResponse<RoomDetailsDto>.Success("Details found", details);
            }
            return ApiResponse<RoomDetailsDto>.Fail("Room not found", 404);
        }
        catch (Exception ex)
        {
            return ApiResponse<RoomDetailsDto>.Fail($"Error: {ex.Message}", 500);
        }
    }
}
}
