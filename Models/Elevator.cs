using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace TheradingOdev.Models
{
    // AVM deki asansörleri temsil eder
    class Elevator
    {
        private AVM avm; // avm referansı
        public ElevatorMode mode; // asasnsörün modu idle / woking
        public int floor; // asansörün bulunduğu kat
        public int destination; // asansörün hedef katı
        public Direction direction; // asansörün yönü up / down
        public int capacity; // asansörün alabileceği max sayısı
        public int countInside; // mevcut insan sayısı
        public List<Group> inside; // içinde bulunan guruplar

        public Elevator(AVM avm) // yapıcı metod
        {
            this.avm = avm;
            mode = ElevatorMode.idle;
            floor = 0;
            destination = 0;
            direction = Direction.up;
            capacity = 10;
            countInside = 0;
            inside = new List<Group>();
        }
        ///<summary>
        /// Bu metod asansör bi kata ulaştığın da onu doldurur
        ///</summary>
        public void FillElevator(Floor f)
        {
            if(countInside < capacity) // eğer daha tam dolmadıysak
            {
                int passangers = capacity - countInside; // kaç kişi labilirz hesapla
                int index = 0;
                while(index < f.floorQueue.Count) // kattaki tüm asansör bekleyen guruplara bak
                {
                    if(passangers > f.floorQueue[index].peopleCount) // eğer passanger count bu indexteki gurubun sayısından büyükse
                    {
                        inside.Add(new Group((f.floorQueue[index].peopleCount), f.floorQueue[index].targetFloor)); // asansöre guruptaki tüm yolcuları al

                        if (f.floorIndex > 0) // kat 0 değilse kattaki insan sayısından gurubu çıkar
                            f.currentPeopleCount -= f.floorQueue[index].peopleCount;

                        f.peopleInQueue -= f.floorQueue[index].peopleCount; // kuyruktaki insanlardan da çıkar
                        countInside += f.floorQueue[index].peopleCount; // içerdeki insan sayısına ekle

                        passangers -= f.floorQueue[index].peopleCount; // passangerstan binenleri çıkar

                        f.floorQueue.RemoveAt(index); // binen gurubu sıradan sil
                        index += 1; // bi sonraki guruba geç
                    }
                    else if (passangers <= f.floorQueue[index].peopleCount) // eğer bu grubun sayısı passangers sayısından büyükse
                    {
                        inside.Add(new Group((passangers), f.floorQueue[index].targetFloor)); // passanger kadar adamı asansöre ekle
                        f.floorQueue[index].peopleCount -= passangers;

                        if (f.floorIndex > 0)
                            f.currentPeopleCount -= passangers; // kattan o insanları çıkar

                        f.peopleInQueue -= passangers; // sıradaki insanlardan o kadar insanları çıkar 
                        countInside += passangers; // içeri o kadar insanı ekle

                        passangers = 0;

                        break; // döngüden çık buna gerek yok aslında ama tekrar dögü testi yapmasın diye optimizasyon :D
                    }
                }
            }
        }
        ///<summary>
        /// Asansörün gideceği yönü seçer ve destinationu belirler
        ///</summary>
        public void ChooseDirection(List<Floor> floors)
        {
            if(direction == Direction.up) // yukarı gidiyosa
            {
                if(floor == 4) // 4. kattaysa aşağı git
                {
                    direction = Direction.down;
                    destination -= 1; // aşşağı devam et
                }
                else // 4. katta değilse
                {
                    bool goingUp = false; // kontol değişkeni
                    for (int i = floor + 1; i < 5; i++) // gendi bulunudğumuz kattan sonraki katlar için
                    {
                        if (inside.Any(g => g.targetFloor == i) || floors.Any(f => f.floorIndex == i && f.floorQueue.Count > 0)) // o katta inecek veya binece var mı bak
                        {
                            destination += 1; // bi sonraki kata git
                            goingUp = true; // kontrol değişkenini true yap
                            break; // varsa yukarı gitmeye devam et ve döngüden çık.
                        }
                    }
                    if(!goingUp) // eğer kontol değişkeni değişmediyse demektir ki yukarıda iş yok o zaman aşağı git
                    {
                        direction = Direction.down;
                        destination -= 1; // aşşağı devam et
                    }
                }
                
            }
            else if (direction == Direction.down) // eğer aşağı gidiyorsa
            {
                if (floor == 0) // 0. kattaysak yukarı git
                {
                    direction = Direction.up;
                    destination += 1; // bi sonraki kata git
                }
                else // 0 da değilsek
                {
                    bool goingDown = false; // kontrol değişkeni
                    for (int i = floor - 1; i >= 0; i--)
                    {
                        if (inside.Any(g => g.targetFloor == i) || floors.Any(f => f.floorIndex == i && f.floorQueue.Count > 0)) // aşşağımızdaki katlarda inecek veya binecek varmı bak
                        {
                            destination -= 1; // aşşağı devam et
                            goingDown = true; // kontol değişkeninin true yap
                            break; // döngüden çık
                        }
                    }
                    if (!goingDown) // eğer kontrol değişkeni false ise demektir ki aşşağıda işimz yok yukarı git
                    {
                        direction = Direction.up;
                        destination += 1; // bi sonraki kata git
                    }
                }
            }
        }
        ///<summary>
        /// Asansörün yapması gerekn her şeyi yapan fonkston AVM sınıfından çağrılır.
        /// Sırasıyla boşaltır,
        /// doldurur
        /// sonra hareket eder
        /// ve yeni hedefini belirler.
        ///</summary>
        public void MoveElevator(List<Floor> floors)
        {
            EvacuateElevator(floors[floor]);
            FillElevator(floors[floor]);
            floor = destination; // hedef kata git
            ChooseDirection(floors);
        }
        ///<summary>
        /// Bu metod asansör bi kata ulaştığın da o katta inecekleri indirir
        ///</summary>
        public void EvacuateElevator(Floor f)
        {
            int index = 0;
            while(index < inside.Count) //  asansördeki tüm gurupları gez
            {
                if (inside[index].targetFloor == f.floorIndex) // eğer hedef katları bu ise 
                {
                    if (f.floorIndex != 0)
                        f.currentPeopleCount += inside[index].peopleCount; // eğer sıfırncı kat değilse kata ekle
                    else
                    {
                        avm.exitCount += inside[index].peopleCount; // 0. kat ise çıkan insanlara ekle
                    }
                    countInside -= inside[index].peopleCount; // asansörden çıkar
                    
                    inside.RemoveAt(index); // asansörden gurubu sil
                    if (index >= inside.Count) break; // gene gereksiz bi break
                }
                else
                {
                    index += 1; // gurubun katı bu değilse bi sonraki guruba geç
                }
            }
        }
    }
    public enum ElevatorMode
    {
        working, idle
    }
    public enum Direction
    {
        up, down
    }
}
