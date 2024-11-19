using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBookApp.Models;
using MyBookApp.Middleware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ������ ���� � ������
var books = new List<Book>
{
    new Book { Id = 1, Title = "���������", Category = "music" },
    new Book { Id = 2, Title = "������ �� �������", Category = "science" },
    new Book { Id = 3, Title = "��������� �����", Category = "history" },
    new Book { Id = 4, Title = "��������� �����", Category = "music" },
};

// ������� ��� ��������� ���� ����
app.MapGet("/allbooks", async context =>
{
    var html = GenerateHtmlTable(books);
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(html);
});

// ������� ��� ��������� ��������
app.MapGet("/", async context =>
{
    var html = GenerateHomePage(context);
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(html);
});

string GenerateHomePage(HttpContext context)
{
    var request = context.Request;
    var port = request.Host.Port ?? 80;

    var html = @"
    <html>
    <head>
        <title>������� ��������</title>
        <meta charset='UTF-8'>
    </head>
    <body>
        <h1>����� ����������!</h1>
        <p>�������� ���� �� ��������� ������:</p>
        <ul>
            <li><a href='/allbooks'>��� �����</a></li>
            <li><a href='/getbooks?token=token12345&category=music'>����� ��������� 'music' (�����������)</a></li>
            <li><a href='/getbooks?token=token12345&category=science'>����� ��������� 'science' (�����������)</a></li>
            <li><a href='/getbooks?token=��������_�����&category=music'>������� ������� � �������� �������</a></li>
        </ul>
    </body>
    </html>
    ";

    return html;
}

app.UseWhen(context => context.Request.Path.StartsWithSegments("/getbooks"), appBuilder =>
{
    appBuilder.UseMiddleware<AuthMiddleware>();

    // ���������� ������� ����� �����������
    appBuilder.Run(async context =>
    {
        var category = context.Request.Query["category"].ToString();
        var filteredBooks = books;

        if (!string.IsNullOrEmpty(category))
        {
            filteredBooks = books.Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var html = GenerateHtmlTable(filteredBooks);
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(html);
    });
});

app.Run();

string GenerateHtmlTable(List<Book> books)
{
    var html = "<table border='1' cellpadding='5' cellspacing='0'>";
    html += "<tr><th>ID</th><th>��������</th><th>���������</th></tr>";

    foreach (var book in books)
    {
        html += $"<tr><td>{book.Id}</td><td>{book.Title}</td><td>{book.Category}</td></tr>";
    }

    html += "</table>";
    return html;
}
