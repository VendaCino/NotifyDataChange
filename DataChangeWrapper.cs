
using System;

using System.Reflection;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;



/*============================================================
**
** Name: NotifyDataChange
**
**
** Purpose: 为被标记的属性setter或方法实现通知
**          用于告知其他用户数据变更
**
**
============================================================*/

namespace Venda.Net.Util
{

    [AttributeUsage(AttributeTargets.Method)]
    class DataChangeAttribute : Attribute {
        static public bool IsDataChangeMethod(MethodInfo mi) {
            var attrs = mi.GetCustomAttributes(false);
            bool result = false;
            foreach (var attr in attrs)
                if (typeof(DataChangeAttribute).IsInstanceOfType(attr)) { result = true; break; }
            return result;
        }
    }


    public delegate void OnDataChangeFunction(IMethodCallMessage methodInfo);


    public class DynamicProxy<T> : RealProxy
    {
        private readonly T _decorated;
        private OnDataChangeFunction _onDataChange;

        public DynamicProxy(T decorated, OnDataChangeFunction onDataChange)
          : base(typeof(T))
        {
            _onDataChange = onDataChange;
            _decorated = decorated;
        }

        private static bool IsValidMethod(MethodInfo methodInfo)
        {
            return DataChangeAttribute.IsDataChangeMethod(methodInfo);
        }
        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = msg as IMethodCallMessage;
            var methodInfo = methodCall.MethodBase as MethodInfo;

            try
            {
                var result = methodInfo.Invoke(_decorated, methodCall.InArgs);
                if (IsValidMethod(methodInfo))
                    _onDataChange(methodCall);
                return new ReturnMessage(result, null, 0,
                  methodCall.LogicalCallContext, methodCall);
            }
            catch (Exception e)
            {
                return new ReturnMessage(e, methodCall);
            }
        }
    }
    public class DataChangeWrapper<T> where T : MarshalByRefObject
    {
        public static T Wrap(T obj, OnDataChangeFunction redirectFunction)
        {
            var dynamicProxy = new DynamicProxy<T>(obj, redirectFunction);
            return dynamicProxy.GetTransparentProxy() as T;
        }
        private static bool IsValidMethod(MethodInfo methodInfo)
        {
            return DataChangeAttribute.IsDataChangeMethod(methodInfo);
        }
    }
    public class Method2IdTable
    {
        private byte sid = 1;
        private System.Collections.Hashtable _funcIdTable;
        public Method2IdTable(Type T)
        {
            _funcIdTable = new System.Collections.Hashtable();
            var mis =T.GetMethods();
            foreach (var mi in mis) if (DataChangeAttribute.IsDataChangeMethod(mi)) _funcIdTable.Add(mi, sid++);
        }
        public byte GetMehtodId(MethodInfo mi)
        {
            if (_funcIdTable.ContainsKey(mi))
                return (byte)_funcIdTable[mi];
            else return 0;
        }
    }
}


