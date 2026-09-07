using Microsoft.AspNetCore.Mvc;
using VnuaCare.Data.Systems.Context;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace VnuaCare.API.Controllers.Business;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly string _connectionString;

    public UserController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    [HttpGet]
    public async Task<IEnumerable<VcUsers>> Get()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            var result = await conn.QueryAsync<VcUsers>("SELECT * FROM vc_users", null, null, null,CommandType.Text);
            
            return result;
        }
    }
}