using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace Venda.Net.Util.Example
{
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
            Console.WriteLine($"{Name}'s age is {Age}");
        }
    }


    public class Example
    {
        static UdpClient udpClient = new UdpClient(11000);


        static public void test()
        {
            udpClient.Connect("127.0.0.1", 11001);
            PeopleData oPeopleData = new PeopleData();
            Method2IdTable method2IdTable = new Method2IdTable(oPeopleData.GetType());
            PeopleData people = DataChangeWrapper<PeopleData>.Wrap(oPeopleData,
            (IMethodCallMessage mcm) =>
            {
                var mi = mcm.MethodBase as MethodInfo;
                var attrs = mi.GetCustomAttributes(false);
                byte mid = method2IdTable.GetMehtodId(mi);
                Console.ForegroundColor = ConsoleColor.Red;
                string argstr = "";
                foreach (var arg in mcm.InArgs)
                {
                    argstr += arg.GetType().Name + ":" + arg + " ";
                }
                Console.WriteLine($"[{mid}] {mi.Name} \targc {mcm.ArgCount} \t[{argstr}]");
                Console.ResetColor();


                List<byte> buffer = new List<byte>();
                buffer.Add(mid);
                foreach (var arg in mcm.InArgs)
                {
                    var T = arg.GetType();
                    buffer.AddRange(ObjectToByte(arg));
                }
                var bytes = buffer.ToArray();
                udpClient.Send(bytes, bytes.Length);

            });
            Console.WriteLine($"bool size :{sizeof(bool)}, char size :{sizeof(char)}");
            Console.WriteLine($"bool size :{Marshal.SizeOf(typeof(bool))}, char size :{Marshal.SizeOf(typeof(char))}");
            people.Age = 18;
            people.Age = 19;
            oPeopleData.Age = 20;//notice this change not be cut in
            people.Name = "XiaoMing";
            people.Sex = false;
            people.AddItem(1);
            people.Add3Item(1, 2, 3);
            people.ShowAge();

            Console.WriteLine($"{people.GetType().Name} ");
        }

        public static byte[] ObjectToByte(object obj)
        {
            //you can use [dynamic] type too at .NET4.0

            List<byte> buffer = new List<byte>();
            if (typeof(String).IsInstanceOfType(obj))
            {
                var str = obj as string;
                return System.Text.Encoding.Unicode.GetBytes(str);
            }
            else if (typeof(bool).IsInstanceOfType(obj))
            {
                return BitConverter.GetBytes((bool)obj);
            }
            else switch (Marshal.SizeOf(obj))
                {
                    case 1: buffer.Add((byte)obj); break;
                    case 2: buffer.AddRange(BitConverter.GetBytes((Int16)obj)); break;
                    case 4: buffer.AddRange(BitConverter.GetBytes((Int32)obj)); break;
                    case 8: buffer.AddRange(BitConverter.GetBytes((Int64)obj)); break;
                }
            return buffer.ToArray();
        }
    }
    
    class Program
    {
        public static void Main()
        {
            //DemoAssemblyBuilder.testmain();
            //Test.TestMain();
            Example.test();
        }
    }
}
