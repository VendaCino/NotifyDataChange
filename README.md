# DataChangeWrapper File
Notify other object when marked setters or methods called.
<br><br><br>


## DataChangeAttribute class
mark setter or method that will change data
### method
|type |name|description|
|-----|----|-----------|
|static|IsDataChangeMethod(MethodInfo mi)|Indicate whether this method is makerd by this attribute|

<br><br><br>


## OnDataChangeFunction delegate
Call this delegate when data change 
<br><br><br>


## DynamicProxy class 
extend:RealProxy

### constructor
|type |name|description|
|-----|----|-----------|
|constructor|DynamicProxy(T decorated, OnDataChangeFunction onDataChange)|Initializes a new instance of the DynamicProxy class to the decorated and OnDataChangeFunction indicated by input . |

<br><br><br>


## DataChangeWrapper<T> class
provide static method to wrap a object.
### method
|type |name|description|
|-----|----|-----------|
|static|Wrap(T obj, OnDataChangeFunction redirectFunction)|return wraped object with OnDataChangeFunction |

<br><br><br>


## Method2IdTable class
record method id(byte type) to indicate which method is called 
### constructor
|type |name|description|
|-----|----|-----------|
|constructor|Method2IdTable(Type T)|Initializes a new instance of the Method2IdTable class with the type indicated by input . |

### method
|type |name|description|
|-----|----|-----------|
|static|GetMehtodId(MethodInfo mi)|return method id(1-255). return 0 if not exist  |

<br><br><br>


## Example
For example, you are building a local multiplayer game.
you have a data class
```
public class PeopleData{
    int X{get;set;};
    int Y{get;set;};
    int Hp{get;set;};
    List<int> items = new List<int>();
    public void AddItem(int it)
    {
        items.Add(it);
    }
}
```

When a people's postion is change, you should call other client to response this change.
A not good but easier way is sending all PeopleData to client, but it costs more network data.
A normal way is only send the changed part. But you should add more method in this way, like sendX(),sendY(). And edit many code when add a new member to PeopleData class.

This method use AOP and Attribute to dynamically notify DataChange. 

```
public class PeopleData{
    int X{get;[DataChange]set;};
    int Y{get;[DataChange]set;};
    int Hp{get;[DataChange]set;};
    List<int> items = new List<int>();
    [DataChange]
    public void AddItem(int it)
    {
        items.Add(it);
    }
}
```
when datachange method is called you can send it to client like byte[]={methodID,arg1,arg2}

```
public static void Main(){
    udpClient.Connect("127.0.0.1", 11001);
    PeopleData oPeopleData = new PeopleData();
    Method2IdTable method2IdTable = new Method2IdTable(oPeopleData.GetType());
    PeopleData people = DataChangeWrapper<PeopleData>.Wrap(oPeopleData,
    (IMethodCallMessage mcm) =>
    {
        var mi = mcm.MethodBase as MethodInfo;
        var attrs = mi.GetCustomAttributes(false);
        byte mid = method2IdTable.GetMehtodId(mi);

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
}
public static byte[] ObjectToByte(object obj)
{
    //if you use .NET4.0+ you can use [dynamic] type 

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
```

In client, use the byte[] data to invoke the method

```
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
```

<br><br><br>
### Reference
#### AOP
https://msdn.microsoft.com/en-us/magazine/dn574804.aspx <br>
https://msdn.microsoft.com/zh-cn/library/dn574804.aspx   (中文)<br>
#### Attributes
https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/attributes/ <br>
https://docs.microsoft.com/zh-cn/dotnet/csharp/programming-guide/concepts/attributes/   (中文)<br>
