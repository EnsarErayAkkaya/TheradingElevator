using System;
using System.Collections.Generic;
using System.Threading;

namespace TheradingOdev.Models
{
    class AVM
    {
        private Object Lock = new Object(); // farklı threadker çalıştığı için aynı anda değişiklik yapıp hatalı işleme yol açmasınlar diye bir lock değişkeni hafızayı biz işlem yaparken kitler ve başka threadler datayı değiştiremez
        public List<Floor> floors; // katlar
        public List<Elevator> elevators; // asansörler
        public int exitCount = 0; // avmden çıkan insan sayısı
        public AVM() // yapıcı metod
        {
            floors = new List<Floor> // katları oluştur
            {
                new Floor(0),
                new Floor(1),
                new Floor(2),
                new Floor(3),
                new Floor(4)
            };
            elevators = new List<Elevator> // asansörleri oluştur
            {
                new Elevator(this),
                new Elevator(this),
                new Elevator(this),
                new Elevator(this),
                new Elevator(this)
            };
        }

        ///<summary>
        ///500 ms zaman aralıklarıyla[1 - 10] arasında rastgele
        ///sayıda müşterinin AVM’ye giriş yapmasını sağlamaktadır(Zemin Kat). Giren
        ///müşterileri rastgele bir kata(1-4) gitmek için asansör kuyruğuna alır
        ///</summary>
        public void AVMLoginThread()
        {
            int interval = 500; // kaç ms de bir çalışacak
            int minPeople = 1, maxPeople = 10; // oluşabilecek min ve max insan sayısı
            int currentFloorIndex = 0; // girilecek kat sayısı mutlak zaten 0 olacak ama ben değişkene atamak istedim
            int minTargetFloor = 1, maxTargetFloor = 4; // hedef olabilecek min ve max kat
            Random r = new Random();
            while (true)
            {
                lock (Lock) // datayı kitle
                {
                    int peopleCount = r.Next(minPeople, maxPeople + 1); // random insan sayısı oluştur

                    int targetFloor = r.Next(minTargetFloor, maxTargetFloor + 1); // random hedef katı seç
                
                
                    floors[currentFloorIndex].floorQueue.Add(new Group(peopleCount, targetFloor)); //kata yeni insan ekle
                    floors[currentFloorIndex].peopleInQueue += peopleCount; // kuyruğa ekle
                }

                Thread.Sleep(interval); // Wait 500 ms
            }
        }

        ///<summary>
        ///1000 ms zaman aralıklarıyla [1-5] arasında rastgele
        ///sayıda müşterinin AVM’den çıkış yapmasını sağlamaktadır(Zemin Kat). Çıkmak
        ///isteyen müşterileri rastgele bir kattan(1-4), zemin kata gitmek için asansör kuyruğuna alır.
        ///</summary>
        public void AVMExitThread()
        {
            int interval = 1000; // kaç ms de bir çalışacak
            int minPeople = 1, maxPeople = 5; // çkabilecek min ve max insan sayısı
            int targetFloorIndex = 0; // çıkan gurubun hedef katı
            int minSelectedFloor = 1, maxSelectedFloor = 4; // seçilecek kat
            Random r = new Random();
            while (true)
            {
                lock (Lock) // datayı kitle
                {
                    int selectedFloor = r.Next(minSelectedFloor, maxSelectedFloor + 1); // bi kat seç
                    if(floors[selectedFloor].currentPeopleCount > 0) // eğer katta insan varsa
                    {
                        int peopleNotInQueue = floors[selectedFloor].currentPeopleCount - floors[selectedFloor].peopleInQueue; // şu anda kuyrukta olmayan insan sayısını bul

                        int exitingPeopleCount = 0;

                
                        if ( peopleNotInQueue < maxPeople) // eğer max çıkabilecek insan sayısından kattaki mevcut insanlar az ise
                        {
                            exitingPeopleCount = r.Next(minPeople, peopleNotInQueue + 1); // maximumu peopleNotInQueue olacak kadar insan çıkar
                        }
                        else // değilse
                        {
                            exitingPeopleCount = r.Next(minPeople, maxPeople + 1); // maximum maxPeople olacak kadar insan çıkar
                        }
                
                        floors[selectedFloor].floorQueue.Add(new Group(exitingPeopleCount, targetFloorIndex)); // çıkacak grubu saraya koy
                        floors[selectedFloor].peopleInQueue += exitingPeopleCount; // sıradaki insansayısına o insanları ekle
                    }
                }
                Thread.Sleep(interval); // Wait 1000 ms
            }
        }

        ///<summary>
        ///Katlardaki kuyrukları kontrol eder.Maksimum kapasiteyi
        ///aşmayacak şekilde kuyruktaki müşterilerin talep ettikleri katlarda taşınabilmesini
        ///sağlar. Bu thread asansör sayısı kadar (5 adet) olmalıdır.
        ///</summary>
        public void AVMElevatorThread()
        {
            int interval = 200; // kaç ms de bir çalışacak
            while (true)
            {
                foreach (Elevator e in elevators) // tüm asansörleri gez
                {
                    if(e.mode == ElevatorMode.working) // çalışan asansörler için
                    {
                        lock (Lock) // datayı kitle
                        {
                            try
                            {
                                e.MoveElevator(floors); // asansör işlemlerini yap
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Exception {0}", ex.Message); // hata varsa consola yaz
                            }
                            
                        }   
                    }
                }
                Thread.Sleep(interval); // Wait 200 ms
            }
        }

        ///<summary>
        ///Katlardaki kuyrukları kontrol eder.Kuyrukta bekleyen kişilerin
        ///toplam sayısı asansörün kapasitesinin 2 katını aştığı durumda (20) yeni asansörü aktif
        ///hale getirir.Kuyrukta bekleyen kişilerin toplam sayısı asansör kapasitenin altına
        ///indiğinde asansörlerden biri pasif hale gelir.Bu işlem tek asansörün çalıştığı durumda
        ///geçerli değildir.
        ///</summary>
        public void AVMControlThread()
        {
            int interval = 500; // çalışma aralığı
            int activeElevatorCount = 1; // aktif asansör sayısı
            while (true)
            {
                lock (Lock) // kitle
                {
                    try
                    {


                        int waitingPeopleCount = 0;
                        foreach (Floor floor in floors)
                        {
                            foreach (Group g in floor.floorQueue)
                            {
                                if (g != null)
                                {
                                    waitingPeopleCount += g.peopleCount; // gurupta kaç kişi var 
                                }
                            }
                        }
                        if (waitingPeopleCount < 20) // 20 den küçükse 1
                        {
                            activeElevatorCount = 1;
                        }
                        else if (waitingPeopleCount >= 20 && waitingPeopleCount < 30)
                        {
                            activeElevatorCount = 2;
                        }
                        else if (waitingPeopleCount >= 30 && waitingPeopleCount < 40) // ...
                        {
                            activeElevatorCount = 3;
                        }
                        else if (waitingPeopleCount >= 40 && waitingPeopleCount < 50)
                        {
                            activeElevatorCount = 4;
                        }
                        else if (waitingPeopleCount >= 50) // 50 den büyükse 5 
                        {
                            activeElevatorCount = 5;
                        }

                        for (int i = 0; i < elevators.Count; i++) // tüm asansörler için 
                        {
                            if (i < activeElevatorCount) // activeElevatorCount kadar asansörü çalıştır
                            {
                                elevators[i].mode = ElevatorMode.working; // asansörü çalıştır
                            }
                            else
                            {
                                if (elevators[i].inside.Count < 1) // eğer içinde yolcu varsa durdurma
                                    elevators[i].mode = ElevatorMode.idle;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception {0}", ex.Message);
                    }
                }
                Thread.Sleep(interval); // Wait 500 ms
            }
        }
    }
}
