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

// Список книг в памяти
var books = new List<Book>
{
    new Book { Id = 1, Title = "Щелкунчик", Category = "music" },
    new Book { Id = 2, Title = "Физика на пальцах", Category = "science" },
    new Book { Id = 3, Title = "Искусство войны", Category = "history" },
    new Book { Id = 4, Title = "Лебединое озеро", Category = "music" },
};

// Маршрут для получения всех книг
app.MapGet("/allbooks", async context =>
{
    var html = GenerateHtmlTable(books);
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(html);
});

// Маршрут для стартовой страницы
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
        <title>Главная страница</title>
        <meta charset='UTF-8'>
    </head>
    <body>
        <h1>Добро пожаловать!</h1>
        <p>Выберите одну из следующих ссылок:</p>
        <ul>
            <li><a href='/allbooks'>Все книги</a></li>
            <li><a href='/getbooks?token=token12345&category=music'>Книги категории 'music' (авторизация)</a></li>
            <li><a href='/getbooks?token=token12345&category=science'>Книги категории 'science' (авторизация)</a></li>
            <li><a href='/getbooks?token=неверный_токен&category=music'>Попытка доступа с неверным токеном</a></li>
        </ul>
    </body>
    </html>
    ";

    return html;
}

app.UseWhen(context => context.Request.Path.StartsWithSegments("/getbooks"), appBuilder =>
{
    appBuilder.UseMiddleware<AuthMiddleware>();

    // Обработчик запроса после авторизации
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
    html += "<tr><th>ID</th><th>Название</th><th>Категория</th></tr>";

    foreach (var book in books)
    {
        html += $"<tr><td>{book.Id}</td><td>{book.Title}</td><td>{book.Category}</td></tr>";
    }

    html += "</table>";
    return html;
}
