using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
namespace Example
{
    [AttributeUsage(AttributeTargets.Method)]
    class DataChangeAttribute : Attribute
    {
        static public bool IsDataChangeMethod(MethodInfo mi)
        {
            var attrs = mi.GetCustomAttributes(false);
            bool result = false;
            foreach (var attr in attrs)
                if (typeof(DataChangeAttribute).IsInstanceOfType(attr)) { result = true; break; }
            return result;
        }
    }
    //------------------example----------------
    public class PeopleData : MarshalByRefObject
    {
        public string Name { [DataChange]set; get; } = "no name";
        public bool Sex { [DataChange]set; get; } = true;
        List<int> items = new List<int>();
        public int Age { [DataChange]set; get; }
        [DataChange]
        public void AddItem(int it)
        {
            items.Add(it);
        }
        [DataChange]
        public void Add3Item(int it1, int it2, int it3)
        {
            items.Add(it1);
            items.Add(it2);
            items.Add(it3);
        }

        public void ShowAge()
        {
            Console.WriteLine($"{Name}'s age is {Age} ,he has {items.Count} item");
        }
    }
    class Program
    {
        private static bool IsValidMethod(MethodInfo methodInfo)
        {
            return DataChangeAttribute.IsDataChangeMethod(methodInfo);
        }
        static void invoke(byte[] bytes, PeopleData people)
        {
            var T = people.GetType();
            var mis=T.GetMethods();

            byte mid = bytes[0];
            int offset = 1;

           
            foreach (var mi in mis)
            {
                if (IsValidMethod(mi)) mid--;
                if(mid == 0)
                {
                    var pis = mi.GetParameters();
                    if (pis.Length == 0) { mi.Invoke(people, null); break; }
                    object[] objs = new object[pis.Length];
                    int i = 0;
                    foreach (var pi in pis)
                    {
                        objs[i++]=ByteToObj(pi.ParameterType, bytes,ref offset);
                    }
                    mi.Invoke(people,objs);
                    break;
                }
            }
        }
        public static object ByteToObj(Type T,byte[] bytes,ref int offset)
        {
            object tmp=null;
            if (T==typeof(String))
            {
                tmp= Encoding.Unicode.GetString(bytes,offset, bytes.Length-offset);
                offset = bytes.Length;
                return tmp;
            }
            else if (T == typeof(bool))
            {
                tmp= bytes[offset]==0 ;
                offset += 1;
                return tmp;
            }
            else switch (Marshal.SizeOf(T))
                {
                    case 1: tmp = bytes[offset];offset+=1; break;
                    case 2: tmp = BitConverter.ToInt16(bytes,offset); offset += 2; break;
                    case 4: tmp = BitConverter.ToInt32(bytes, offset); offset += 4; break;
                    case 8: tmp = BitConverter.ToInt64(bytes, offset); offset += 8; break;
                }
            return tmp;
        }
        static void Main(string[] args)
        {
            PeopleData people=new PeopleData();
            UdpClient udpClient = new UdpClient(11001);
            try
            {

                //IPEndPoint object will allow us to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                for (int i = 0; i < 6; i++)
                {
                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    invoke(receiveBytes, people);
                    people.ShowAge();
                }

                udpClient.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
