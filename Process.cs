using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3
{
    public class Process
    {
        private static int nextID = 0;
        public int Id { get; set; }
        public int AllocatedCP { get; set; }
        public int AllocatedMemory { get; set; }
        public DateTime creationTime { get; set; }
        public int executeTime { get; set; }
        public int waitingTime { get; set; }
        public int priorityLvl { get; set; }    

        public Process(int CP, int Memory) 
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            Id = ++nextID;
            AllocatedCP = CP;    
            AllocatedMemory = Memory;    
            creationTime = DateTime.Now;
            executeTime = rnd.Next(10,30);
            priorityLvl = rnd.Next(1,10);
        }

        public override string ToString()
        {
            return $"№{Id} <{creationTime:T}> -{AllocatedCP}-/-{AllocatedMemory}- >> {executeTime}";
        }

        public static int sortPlan(Process process1, Process process2)
        {
            return process1.priorityLvl - process2.priorityLvl; 
        }

        public static int sortHRN(Process process1, Process process2)
        {
            double first = ((double)process1.waitingTime + (double)process1.executeTime) / (double)process1.executeTime;
            double second = ((double)process2.waitingTime + (double)process2.executeTime) / (double)process2.executeTime;

            if (first > second)
                return 1;
            else if (first < second)
                return -1;
            else return 0;
        }
    }
}
