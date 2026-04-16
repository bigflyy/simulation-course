using System;
using System.Collections.Generic;

namespace SimulationLabs
{

    /// Заявка (клиент) в системе массового обслуживания.
    public class Request
    {
        public int Id { get; set; }
        public double ArrivalTime { get; set; }
        public double ServiceTime { get; set; }
        public double MaxWaitTime { get; set; } // максимальное время ожидания (нетерпеливость)

        public Request(int id, double arrivalTime, double serviceTime, double maxWaitTime)
        {
            Id = id;
            ArrivalTime = arrivalTime;
            ServiceTime = serviceTime;
            MaxWaitTime = maxWaitTime;
        }
    }


    /// Сервер (прибор) — обрабатывает заявки.
    public class Server
    {
        public int Id { get; set; }
        public double FreeAt { get; set; } // момент, когда сервер освободится

        public bool IsBusy(double currentTime) => currentTime < FreeAt;

        public Server(int id)
        {
            Id = id;
            FreeAt = 0;
        }
    }


    /// Система массового обслуживания M/M/n с очередью и нетерпеливыми заявками.
    /// Два усложнения:
    ///   1) Очередь ограниченной длины (maxQueueSize)
    ///   2) Нетерпеливые заявки — покидают очередь, если ждут дольше maxWaitTime

    public class QueueSystem
    {
        public int NumServers { get; }
        public int MaxQueueSize { get; }
        public double Lambda { get; }
        public double Mu { get; }
        public double MaxPatience { get; } // максимальное время ожидания заявки

        public List<Server> Servers { get; }
        public Queue<Request> WaitingQueue { get; }

        // Статистика
        public int TotalArrivals { get; private set; }
        public int TotalServed { get; private set; }
        public int TotalRefused { get; private set; } // отказ из-за переполнения очереди
        public int TotalImpatient { get; private set; } // ушли из-за нетерпеливости
        public double TotalWaitTime { get; private set; }
        public double TotalServiceTime { get; private set; }

        // Лог событий
        public List<string> EventLog { get; }

        public QueueSystem(int numServers, int maxQueueSize, double lambda, double mu, double maxPatience)
        {
            NumServers = numServers;
            MaxQueueSize = maxQueueSize;
            Lambda = lambda;
            Mu = mu;
            MaxPatience = maxPatience;

            Servers = new List<Server>();
            for (int i = 0; i < numServers; i++)
                Servers.Add(new Server(i + 1));

            WaitingQueue = new Queue<Request>();

            ResetStats();
            EventLog = new List<string>();
        }

        private void ResetStats()
        {
            TotalArrivals = 0;
            TotalServed = 0;
            TotalRefused = 0;
            TotalImpatient = 0;
            TotalWaitTime = 0;
            TotalServiceTime = 0;
        }

 
        /// Прибытие новой заявки.
        public void Arrival(Request request, double currentTime)
        {
            TotalArrivals++;

            // Ищем свободный сервер
            Server? freeServer = null;
            foreach (var s in Servers)
            {
                if (!s.IsBusy(currentTime))
                {
                    freeServer = s;
                    break;
                }
            }

            if (freeServer != null)
            {
                // Сервер свободен — сразу обслуживаем
                freeServer.FreeAt = currentTime + request.ServiceTime;
                TotalServed++;
                TotalServiceTime += request.ServiceTime;
                Log(currentTime, $"Заявка #{request.Id}: сервер #{freeServer.Id}, обслуживание {request.ServiceTime:F2}");
            }
            else if (WaitingQueue.Count < MaxQueueSize)
            {
                // Очередь не полна — встаём в очередь
                WaitingQueue.Enqueue(request);
                Log(currentTime, $"Заявка #{request.Id}: в очередь (позиция {WaitingQueue.Count}), макс. ожидание {request.MaxWaitTime:F2}");
            }
            else
            {
                // Очередь полна — отказ
                TotalRefused++;
                Log(currentTime, $"Заявка #{request.Id}: ОТКАЗ (очередь полна)");
            }
        }

 
        /// Проверка нетерпеливых заявок: те, кто ждал слишком долго, уходят.
        public void CheckImpatient(double currentTime)
        {
            var stillWaiting = new Queue<Request>();
            while (WaitingQueue.Count > 0)
            {
                var req = WaitingQueue.Dequeue();
                double waited = currentTime - req.ArrivalTime;
                if (waited >= req.MaxWaitTime)
                {
                    TotalImpatient++;
                    Log(currentTime, $"Заявка #{req.Id}: ушла (нетерпеливость), ждала {waited:F2}");
                }
                else
                {
                    stillWaiting.Enqueue(req);
                }
            }
            // Переносим оставшиеся обратно
            while (stillWaiting.Count > 0)
                WaitingQueue.Enqueue(stillWaiting.Dequeue());
        }

 
        /// Освобождение серверов: пытаемся начать обслуживание заявок из очереди.
        public void TryServeFromQueue(double currentTime)
        {
            foreach (var server in Servers)
            {
                if (server.IsBusy(currentTime)) continue;
                if (WaitingQueue.Count == 0) continue;

                var req = WaitingQueue.Dequeue();
                server.FreeAt = currentTime + req.ServiceTime;
                TotalServed++;
                TotalServiceTime += req.ServiceTime;
                double waited = currentTime - req.ArrivalTime;
                TotalWaitTime += waited;
                Log(currentTime, $"Заявка #{req.Id}: сервер #{server.Id}, ждал {waited:F2}, обслуживание {req.ServiceTime:F2}");
            }
        }

 
        /// Число занятых серверов в данный момент.
        public int BusyServers(double currentTime)
        {
            int count = 0;
            foreach (var s in Servers)
                if (s.IsBusy(currentTime)) count++;
            return count;
        }

        private void Log(double time, string message)
        {
            EventLog.Add($"t={time:F2} | {message}");
        }
    }
}
