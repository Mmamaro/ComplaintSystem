using ComplaintSystem.Data;
using ComplaintSystem.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using static QRCoder.PayloadGenerator;

namespace ComplaintSystem.Repositories
{
    public interface IComplaint
    {
        public Task<List<ComplaintResponse>> GetAllComplaints();
        public Task<List<ComplaintResponse>> GetComplaintsByFilters(ComplaintFilters payload);
        public Task<List<ComplaintResponse>> GetComplaintsByManagerDeptId(Guid id);
        public Task<ComplaintResponse> GetComplaintsById(Guid id);
        public Task<List<ComplaintResponse>> GetComplaintsByReporterId(Guid id);
        public Task<List<ComplaintResponse>> GetComplaintsByAccusedId(Guid id);
        public Task<bool> AddComplaint(Complaint payload);
        public Task<bool> UpdateComplaint(Guid id, Guid accused, string ComplaintDescription);
        public Task<bool> ManagerUpdateComplaint(Guid id, ManagerUpdateComplaint payload);
    }
    public class ComplaintRepo : IComplaint
    {
        private readonly IDapperContext _dbContext;
        private readonly ILogger<ComplaintRepo> _logger;

        public ComplaintRepo(IDapperContext dbContext, ILogger<ComplaintRepo> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<bool> AddComplaint(Complaint payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("ReporterId", payload.ReporterId);
                parameters.Add("AcusedId", payload.AccusedId);
                parameters.Add("Complaint", payload.ComplaintDescription);
                parameters.Add("StatusId", payload.StatusId);
                parameters.Add("CreatedOn", payload.CreatedOn);
                parameters.Add("UpdatedOn", payload.UpdatedOn);


                var command = @"INSERT INTO Complaints([ReporterId],[AccusedId],[Complaint],[StatusId],[CreatedOn],[UpdatedOn])
                                VALUS(@ReporterId,@AccusedId,@Complaint,@StatusId,@CreatedOn,@UpdatedOn)";

                return await _dbContext.ExecuteCommand(command, parameters);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in the ComplaintRepo while trying to Add a complaint");
                throw;
            }
        }

        public async Task<List<ComplaintResponse>> GetAllComplaints()
        {
            try
            {
                var query = @"SELECT c.Id,r.Email as Reporter, a.Email as Accussed,c.Complaint, s.Name as Status, 
                              c.ResolutionNote, c.CreatedOn, c.UpdatedOn
                              FROM Complaints as c
                              INNER JOIN Users as r ON c.ReporterId = r.Id
                              INNER JOIN Users as a ON c.AccusedId = a.Id
                              INNER JOIN Statuses as s ON c.StatusId = s.Id";

                var data = await _dbContext.Query<ComplaintResponse>(query);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while trying to get all complaints");
                throw;
            }
        }

        public async Task<List<ComplaintResponse>> GetComplaintsByAccusedId(Guid id)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);

                var query = @"SELECT c.Id,r.Email as Reporter, a.Email as Accussed,c.Complaint, s.Name as Status, 
                              c.ResolutionNote, c.CreatedOn, c.UpdatedOn
                              FROM Complaints as c
                              INNER JOIN Users as r ON c.ReporterId = r.Id
                              INNER JOIN Users as a ON c.AccusedId = a.Id
                              INNER JOIN Statuses as s ON c.StatusId = s.Id
                              WHERE r.AccusedId = @id";

                var data = await _dbContext.QueryWithParams<ComplaintResponse>(query, parameters);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in the ComplaintRepo in the GetComplaintsByAccusedId method");
                throw;
            }
        }

        public async Task<List<ComplaintResponse>> GetComplaintsByFilters(ComplaintFilters payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("start", payload.StartDate);
                parameters.Add("end", payload.EndDate);

                var query = @"SELECT c.Id,r.Email as Reporter, a.Email as Accussed,c.Complaint, s.Name as Status, 
                              c.ResolutionNote, c.CreatedOn, c.UpdatedOn
                              FROM Complaints as c
                              INNER JOIN Users as r ON c.ReporterId = r.Id
                              INNER JOIN Users as a ON c.AccusedId = a.Id
                              INNER JOIN Statuses as s ON c.StatusId = s.Id
                              WHERE c.CreatedOn BETWEEN @start AND @end";

                if (payload.Reporter != null)
                {
                    parameters.Add("reporter", payload.Reporter);
                    query += " AND r.Email = @reporter";
                }

                if (payload.Accused != null)
                {
                    parameters.Add("accused", payload.Accused);
                    query += " AND a.Email = @accused";
                }

                if (payload.Status != null)
                {
                    parameters.Add("status", payload.Accused);
                    query += " AND s.Name = @status";
                }

                var data = await _dbContext.QueryWithParams<ComplaintResponse>(query, parameters);

                return data;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while trying to get complaints by filters");
                throw;
            }
        }

        public async Task<ComplaintResponse> GetComplaintsById(Guid id)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);

                var query = @"SELECT c.Id,r.Email as Reporter, a.Email as Accussed,c.Complaint, s.Name as Status, 
                              c.ResolutionNote, c.CreatedOn, c.UpdatedOn
                              FROM Complaints as c
                              INNER JOIN Users as r ON c.ReporterId = r.Id
                              INNER JOIN Users as a ON c.AccusedId = a.Id
                              INNER JOIN Statuses as s ON c.StatusId = s.Id
                              WHERE c.Id= @id";

                var data = await _dbContext.QuerySingleRecord<ComplaintResponse>(query, parameters);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in the ComplaintRepo in the GetComplaintsById method");
                throw;
            }
        }

        public async Task<List<ComplaintResponse>> GetComplaintsByManagerDeptId(Guid id)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);

                var query = @"SELECT c.Id,r.Email as Reporter, a.Email as Accussed,c.Complaint, s.Name as Status, 
                              c.ResolutionNote, c.CreatedOn, c.UpdatedOn
                              FROM Complaints as c
                              INNER JOIN Users as r ON c.ReporterId = r.Id
                              INNER JOIN Users as a ON c.AccusedId = a.Id
                              INNER JOIN Statuses as s ON c.StatusId = s.Id
                              WHERE r.DepartmentId = @id";

                var data = await _dbContext.QueryWithParams<ComplaintResponse>(query, parameters);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in the ComplaintRepo while trying to get complaints by manager dept id");
                throw;
            }
        }

        public async Task<List<ComplaintResponse>> GetComplaintsByReporterId(Guid id)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);

                var query = @"SELECT c.Id,r.Email as Reporter, a.Email as Accussed,c.Complaint, s.Name as Status, 
                              c.ResolutionNote, c.CreatedOn, c.UpdatedOn
                              FROM Complaints as c
                              INNER JOIN Users as r ON c.ReporterId = r.Id
                              INNER JOIN Users as a ON c.AccusedId = a.Id
                              INNER JOIN Statuses as s ON c.StatusId = s.Id
                              WHERE r.ReporterId = @id";

                var data = await _dbContext.QueryWithParams<ComplaintResponse>(query, parameters);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in the ComplaintRepo in the GetComplaintsByReporterId method");
                throw;
            }
        }

        public async Task<bool> ManagerUpdateComplaint(Guid id, ManagerUpdateComplaint payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);
                parameters.Add("statusId", payload.StatusId);
                parameters.Add("resolutionNote", payload.ResolutionNote);
                parameters.Add("UpdatedOn", DateTime.Now);

                var command = @"UPDATE Complaints SET StatusId = @statusId, ResolutionNote = @resolutionNote, UpdatedOn = @UpdatedOn";

                return await _dbContext.ExecuteCommand(command, parameters);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in the ComplaintRepo in the ManagerUpdateComplaint method");
                throw;
            }
        }

        public async Task<bool> UpdateComplaint(Guid id, Guid accused, string ComplaintDescription)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);
                parameters.Add("accused", accused);
                parameters.Add("Complaint", ComplaintDescription);

                var command = @"UPDATE Complaints SET Accused = @accused, Complaint = @Complaint";

                return await _dbContext.ExecuteCommand(command, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in the ComplaintRepo in the UpdateComplaint method");
                throw;
            }
        }
    }
}
