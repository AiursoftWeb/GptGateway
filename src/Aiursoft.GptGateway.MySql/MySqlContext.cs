using Aiursoft.GptGateway.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.GptGateway.MySql;

public class MySqlContext(DbContextOptions<MySqlContext> options) : GptGatewayDbContext(options);