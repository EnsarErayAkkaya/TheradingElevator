using System;
using System.Collections.Generic;
using System.Text;

namespace TheradingOdev.Models
{
    //Bu sınıf AVM ye giren insan guruplarını temsil eder
    class Group
    {
        public int peopleCount; // guruptaki insan sayısı
        public int targetFloor; // gurubun hedef katı
        
        public Group() // yapıcı metod
        {
            this.peopleCount = 0;
            this.targetFloor = 0;
        }
        public Group(int count, int targetFloor){ // yapıcı metod
            this.peopleCount = count;
            this.targetFloor = targetFloor;
        }
        
    }
}
