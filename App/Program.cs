using App;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, arg) =>  
{
    Console.WriteLine("\n[~] Отмена операции...");
    arg.Cancel = true;
    cts.Cancel();
};

var uris = Input.GetUris();
var destFile = Input.GetOutputFile();
var destStream = destFile.OpenWrite(); // Поток для записи

using var http = new HttpClient(); // Переместили выше, т.к. не нужно создавать HttpClient для каждого потока
// Используем семафор для потокобезопасности
var semaphore = new SemaphoreSlim(1, 1);

try
{
    await Parallel.ForEachAsync(uris, cts.Token, async (uri, ct) =>
    {
        try
        {
            await using var contentStream = await http.GetStreamAsync(uri, ct);
            await semaphore.WaitAsync(ct);
            try
            {
                await contentStream.CopyToAsync(destStream, ct);
            }
            finally
            {
                semaphore.Release();
            }
        }
        
        catch (HttpRequestException ex) // Ошибка HTTP-запроса
        {
            Console.WriteLine($"[-] Ошибка при чтении файла {uri}: {ex.Message}. Файл игнорируется.");
        }
        catch (IOException ex) // Ошибка ввода-вывода
        {
            Console.WriteLine($"[-] Ошибка ввода-вывода при чтении {uri}: {ex.Message}. Файл игнорируется.");
        }
        catch (OperationCanceledException)
        {
            // Пробрасываем выше для обработки отмены всей операции
            throw;
        }
        catch (Exception ex) // Любые другие ошибки
        {
            Console.WriteLine($"[-] Неизвестная ошибка при чтении {uri}: {ex.Message}. Файл игнорируется.");
        }
    });
}
catch (OperationCanceledException)
{
    await Misc.CleanUpOnCancellation(destFile,  destStream);
    Console.WriteLine("[!] Программа экстренно завершает свою работу.");
    return; // Выходим из программы
}
finally
{
    // закрываем FileStream асинхронно
    await destStream.DisposeAsync();
}

Console.WriteLine($"[+] Загрузка завершена. Файл сохранен: {destFile.FullName}");
int lineCount = await Misc.CountLinesAsync(destFile);
Console.WriteLine($"Количество строк в полученном файле: {lineCount}");




