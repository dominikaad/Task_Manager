
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TaskFlow.Models;

namespace TaskFlow.Services
{
    public static class TaskStorageService
    {
        // Путь к файлу с сохранёнными задачами
        private static readonly string FolderPath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "TaskFlow"
            );

        private static readonly string FilePath =
            Path.Combine(FolderPath, "tasks.json");

        // Загрузка задач
        public static List<TaskItem>? LoadTasks()
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            string json = File.ReadAllText(FilePath);

            return JsonSerializer.Deserialize<List<TaskItem>>(json)
                   ?? new List<TaskItem>();
        }

        // Сохранение задач
        public static void SaveTasks(IEnumerable<TaskItem> tasks)
        {
            Directory.CreateDirectory(FolderPath);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(tasks, options);

            File.WriteAllText(FilePath, json);
        }
    }
}
