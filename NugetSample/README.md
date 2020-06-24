# 使用 nuget server 的 API 来实现搜索安装 nuget 包

## Intro

nuget 现在几乎是 dotnet 开发不可缺少的一部分了，还没有用过 nuget 的就有点落后时代了，还不快用起来



nuget 是 dotnet 里的包管理机制，类似于前端的 npm ，php 的 composer，java 里的 maven ...



我觉得包管理机制是现代化编程的必备

nuget 提供了一套包管理机制，有一套关于 nuget server 的规范，使得用户可以自己实现一个 nuget server，

正是这些规范，使得我们可以根据这些规范来实现 nuget server 的包管理的功能，今天主要介绍一下，根据 nuget server 的 api 规范使用原始的 HTTP 请求来实现 nuget 包的搜索和使用 nuget 提供的客户端 SDK 来实现 nuget 包的搜索和下载



## Reference

- <https://github.com/WeihanLi/SamplesInPractice/blob/master/NugetSample/RawApiSample.cs>
- <https://github.com/WeihanLi/SamplesInPractice/blob/master/NugetSample/NugetClientSdkSample.cs>
- <https://docs.microsoft.com/en-us/nuget/reference/nuget-client-sdk>
- <https://docs.microsoft.com/en-us/nuget/create-packages/set-package-type>
- <https://docs.microsoft.com/en-us/nuget/api/overview>
- <https://docs.microsoft.com/en-us/nuget/api/search-autocomplete-service-resource>