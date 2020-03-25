# Nssm Assist Wpf
**NssmAssistWpf这是一个基于NSSM/Json配置文件灵活注册客户端成为后台常驻服务的WPF程序**

# 开发进度
目前只支持两个功能
- 安装服务
- 卸载服务

所以代码量很少，暂时没有添加其他功能的想法

# 用法指南
## 下载 & 编译
程序已经打包好生成X86和X64，可以在Releases处直接下载

如果需要自行生成，可以Clone代码库之后直接Build整个解决方案会自动拉取相关依赖项目的

## 配置 & 使用
下载程序之后编辑 **ServiceInfo.json**，填好如下三项
- ServiceName：服务名称
- ServiceProgramPath：服务运行的程序
- ServiceProcessAlias：程序别名（如服务停止之后需要杀死相关非服务进程则填写该项，否则不填即可）

其中 **ServiceProgramPath** 可以填写绝对路径与相对路径，相对路径使用 ./ 代表当前运行程序的目录下，之后直接运行程序点击安装服务即可

# 示例
修改配置文件如下
```Json
{
  "ServiceName": "Hello world service",
  "ServiceProgramPath": "./hello_world.exe",
  "ServiceProcessAlias": ""
}
```

目录结构如下
- Newtonsoft.Json.dll
- nssm.exe
- NssmAssistUI.exe
- ServiceInfo.json
- hello_world.exe

运行程序点击安装服务后  hello_world.exe 就会在后台常驻运行

# Bug/建议
如果对程序有任何使用Bug/建议欢迎提交ISSUE



