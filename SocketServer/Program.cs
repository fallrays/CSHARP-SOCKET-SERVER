using SocketServer.Network;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

ThreadPool.GetMinThreads(out int minThreads, out int maxThreads);
ThreadPool.SetMinThreads(minThreads, minThreads);
ThreadPool.SetMaxThreads(maxThreads, maxThreads);

NetworkManager.Instance.Init();
NetworkManager.Instance.SocketListen();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
