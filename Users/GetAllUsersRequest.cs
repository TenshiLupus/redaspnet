public class GetAllUsersRequest
{
    public int? Page { get; set; }
    public int? RecordsPerPage { get; set; }
    
    public string? UsernameContains { get; set; }
}
