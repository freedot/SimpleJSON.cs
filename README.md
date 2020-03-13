用法示例

1、构建JSONObject

JSONObject person = new JSONObject();
person["name"] = new JSONObject("jack");
person["desc"] = new JSONObject("hello world \" ... \"!");
person["age"] = new JSONObject(18);
person["married"] = new JSONObject(false);

JSONArray travelFootprint = new JSONArray();
travelFootprint[0] = new JSONObject("China");
travelFootprint[1] = new JSONObject("India");

person["travelFootprint"] = travelFootprint;

Debug.Log(person.ToJSONString());

// 转成JSONString输出
output# {"name":"jack","desc":"hello world \" ... \"!","age":18,"travelFootprint":["China","India"]}


2、解析json格式字符串

string s = {"name":"jack","desc":"hello world \" ... \"!","age":18,"travelFootprint":["China","India"]}；
JSONObject person = JSONObject.Parse(s);

//获取person.name
person["name"].ToString(); // 方法一
person.GetString("name");  // 方法二


