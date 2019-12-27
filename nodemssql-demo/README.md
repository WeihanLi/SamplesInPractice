# nodemssql demo

## Intro

node mssql 使用示例

做了一个简单的封装，使用前请先看一下封装的代码

## GetStarted

1. 创建测试表

    创建表 sql:

    ``` sql
    CREATE TABLE [dbo].[TestEntities]
    (
    [Id] [int] NOT NULL IDENTITY(1, 1) PRIMARY KEY,
    [Extra] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CreatedAt] [datetime2] NOT NULL
    )
    ```

2. 配置数据库连接信息

    js 版本的 demo，数据库连接信息在 `dataaccess/dbUtil.js` 文件中

    ts 版本的 demo，数据库连接信息在 `config/default.json` 文件中

3. 启动 demo

  在要运行的 demo 的目录运行 `npm run start` 即可

## More

如果本地没有 Sql Server 数据库用以测试，可以通过 docker 运行一个 mssql-server,

``` bash
docker run -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=Admin888' -p 1433:1433 --name sqlserver --restart=always -d microsoft/mssql-server-linux:2017-latest
```

## Contact

Contact me with <weihanli@outlook.com> if you need
