### 1、构建JSONObject

```c#
JSONObject person = new JSONObject();
person["name"] = new JSONObject("jack");
person["desc"] = new JSONObject("hello world \" ... \"!");
person["age"] = new JSONObject(18);

JSONArray travelFootprint = new JSONArray();
travelFootprint[0] = new JSONObject("China");
travelFootprint[1] = new JSONObject("India");

person["travelFootprint"] = travelFootprint;

string jsonStr = person.ToJSONString();
Debug.Log(jsonStr);

// 转成JSONString输出
output# {"name":"jack","desc":"hello world \" ... \"!","age":18,"travelFootprint":["China","India"]}
```

### 2、解析json格式字符串

```c#
JSONObject person = JSONObject.Parse(jsonStr);

//获取person.name
person["name"].ToString(); // 方法一
person.GetString("name");  // 方法二
```

