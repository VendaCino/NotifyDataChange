# NotifyDataChange File
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
