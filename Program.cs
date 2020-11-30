using System;
using System.Collections.Generic;
using System.Threading;
using TheradingOdev.Models;
namespace TheradingOdev
{
    class Program
    {
        public static void Main(string[] args)
        {
            AVM avm = new AVM(); // avm objesini tanımla 

            Thread loginThread = new Thread(new ThreadStart(avm.AVMLoginThread)); // fonksyonları threadlere bağla
            Thread exitThread = new Thread(new ThreadStart(avm.AVMExitThread));
            Thread elevatorThread = new Thread(new ThreadStart(avm.AVMElevatorThread)); // ... 
            Thread controlThread = new Thread(new ThreadStart(avm.AVMControlThread));

            try // hata olursa diye önlem
            {
                loginThread.Start(); // threadi başlat
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0}", ex.Message); // hatayı ekrana bas
            }

            try
            {
                exitThread.Start(); // threadi başlat
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0}", ex.Message);
            }

            try
            {
                elevatorThread.Start(); // threadi başlat
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0}", ex.Message);
            }

            try
            {
                controlThread.Start(); // threadi başlat
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0}", ex.Message);
            }

            while (true)
            {
                foreach (Floor floor in avm.floors) // her floor için floor datasını ekrana bas
                {
                    Console.Write(floor.floorIndex + ". floor :");

                    if (floor.currentPeopleCount > 0)
                        Console.Write("all : " + floor.currentPeopleCount);

                    Console.Write(" queue : " + floor.peopleInQueue + "\n");
                }
                Console.WriteLine("exit count : " + avm.exitCount); // şu ana kadar kaç kişi çıktı ekrana bas

                foreach (Elevator elevator in avm.elevators) // asansör bilgilerini ekrana bas
                {
                    Console.WriteLine(elevator.mode == ElevatorMode.working ? "active :True" : "active :False");
                    Console.WriteLine(elevator.mode == ElevatorMode.working ? "             mode :working" : "             mode :idle");
                    Console.WriteLine("             floor:" + elevator.floor);
                    Console.WriteLine("             destination:" + elevator.destination);
                    Console.WriteLine(elevator.direction == Direction.up ? "             direction :up" : "              direction :down");
                    Console.WriteLine("             capacity:" + elevator.capacity);
                    Console.WriteLine("             count_inside:" + elevator.countInside);

                    string s = "[";
                    for (int i = 0; i < elevator.inside.Count; i++)
                    {
                        s += "(" + elevator.inside[i].peopleCount + ", " + elevator.inside[i].targetFloor;
                        s += i < elevator.inside.Count - 1 ? ")," : ")";
                    }
                    s += "]";

                    Console.WriteLine("             inside:" + s); // asamsörün içindeki gurupları ekrana basar
                }
                foreach (Floor floor in avm.floors) // katlarda bekleyen gurupları ekrana bas
                {
                    string s = "[";
                    for (int i = 0; i < floor.floorQueue.Count; i++)
                    {
                        s += "(" + floor.floorQueue[i].peopleCount + ", " + floor.floorQueue[i].targetFloor;
                        s += i < floor.floorQueue.Count - 1 ? ")," : ")";
                    }
                    s += "]";
                    Console.WriteLine(floor.floorIndex + ". floor : " + s);
                }
                Thread.Sleep(200); // 20 ms bekle çok sık yenilemesini istemiyosan 200 ü değiştirebilirsin örneğin 500 e falan
                //Console.Clear(); // eğer sürekli consola alt alta yazmasın uzamasın konsol diyosan bu kodu aç
            }
        }
    }
}
