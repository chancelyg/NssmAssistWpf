# Nssm Assist Wpf
**NssmAssistWpf这是一个基于NSSM/Json配置文件灵活注册客户端成为后台常驻服务的WPF程序**，支持把本地EXE/BAT等执行文件注册成后台服务，也支持从指定HTTP网站下载ZIP压缩包解压并注册成后台服务

界面UI基于WPF开发，简洁明了，适合普通基础用户安装程序后台服务

# 开发进度
目前支持如下3个功能
- 将本地客户端注册成服务
- 从HTTP链接导入压缩包解压之后将客户端注册服务
- 卸载注册服务


# 用法指南
## 下载 & 编译
程序已经打包好生成X86和X64，可以在Releases处直接下载

如果需要自行生成，可以Clone代码库之后直接Build整个解决方案会自动拉取相关依赖项目生成程序

服务列表取决于 **ProgramArgs.json文件**，参数说明如下表
| 参数                  | 类型   | 说明                                               |
| --------------------- | ------ | -------------------------------------------------- |
| ProgramTitle          | String | 程序标题                                           |
| NSSMInfo.Url          | String | NSSM路径（可使用相对路径/绝对路径/HTTP路径）       |
| NSSMIfo.HttpAuthBasic | String | NSSM通过HTTP导入时的用户密码验证，格式 user:passwd |
| ServiceAlias          | String | 服务显示别名                                       |
| ServiceName           | String | 服务运行名称                                       |
| ServiceProgramPath    | String | 程序目录（可使用相对路径/绝对路径/HTTP路径）       |
| ServiceProgramName    | String | 执行文件名称（包含扩展名）                         |
| HttpAuthBasic         | String | 服务通过HTTP导入时的用户密码验证，格式 user:passwd |
| DependentService      | String | 程序安装时检查依赖服务名称                         |

对于所有实际引用到的文件例如NSSM.exe/ProgramArgs.json/等文件都支持使用HTTP链接的，可参考下面的例子

## 使用例子
例1 程序均位于本地：允许注册一个在当前目录下的HelloWorld.exe程序为后台服务，假设NSSM.exe程序也在当前目录下
目录结构应如下
- NssmAssistWpf.exe
- Newtonsoft.Json.dll
- ProgramArgs.json
- Config.json
- NSSM.exe
- HelloWorldDir/
  - HelloWorld.exe

Config.json文件
```Json
{
  "ProgramArgsUrl": "./ProgramArgs.json",
  "HttpAuthBasic": null
}
```

ProgramArgs.json文件
```Json
{
  "ProgramTitle": "HelloWorld",
  "NSSMInfo": {
    "Url": "./NSSM.exe",
    "HttpAuthBasic": ""
  },
  "Services": [
    {
      "ServiceAlias": "HelloWorld服务",
      "ServiceName": "HelloWorld",
      "ServiceProgramPath": "./HelloWorldDir",
      "ServiceProgramName": "HelloWorld.exe",
      "HttpAuthBasic": "",
      "DependentService": null
    }
  ]

}
```

例2 程序均位于HTTP服务器上：允许注册一个位于HTTP服务器上的HelloWorld.exe程序为后台服务，假设NSSM.exe程序也在HTTP服务器上
目录结构应如下
- NssmAssistWpf.exe
- Newtonsoft.Json.dll
- ICSharpCode.SharpZipLib.dll
- Config.json

Config.json文件
```Json
{
  "ProgramArgsUrl": "http://www.example.com/files/ProgramArgs.json",
  "HttpAuthBasic": "example:example"
}
```

ProgramArgs.json文件(存在于HTTP服务器上)
```Json
{
  "ProgramTitle": "HelloWorld",
  "NSSMInfo": {
    "Url": "http://www.example.com/files/NSSM.exe",
    "HttpAuthBasic": "example:example"
  },
  "Services": [
    {
      "ServiceAlias": "HelloWorld服务",
      "ServiceName": "HelloWorld",
      "ServiceProgramPath": "http://www.example.com/files/HelloWorldDir",
      "ServiceProgramName": "HelloWorld.exe",
      "HttpAuthBasic": "example:example",
      "DependentService": null
    }
  ]

}
```

运行程序点击安装服务后  hello_world.exe 就会在后台常驻运行，效果如图

![](https://image.chancel.ltd/2020/03/27/e93c01ee14783.png)

# Bug/建议
如果对程序有任何使用Bug/建议欢迎提交ISSUE



