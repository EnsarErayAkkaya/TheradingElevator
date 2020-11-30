using System;
using System.Collections.Generic;
namespace TheradingOdev.Models
{
    // AVM deki katları temsil eder
    class Floor
    {
        public int floorIndex; // kat indexi
        public int currentPeopleCount; // kattaki insan sayısı
        public List<Group> floorQueue; // katta asansör bekleyen gurup listesi
        public int peopleInQueue; // asansör bekleyen insan sayısı
        public Floor(int floorIndex) // yapıcı metod
        {
            this.floorIndex = floorIndex;
            this.currentPeopleCount = 0;
            floorQueue = new List<Group>();
            this.peopleInQueue = 0;
        }
    }
}
