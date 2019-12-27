import * as log4js from "log4js";
import * as config from "config";

const sleep = require("sleepjs");
const logger = log4js.getLogger("dbUtil");

import * as mssql from "mssql";
import { ConnectionPool, IResult, Request } from "mssql";
import { RetryUtil } from "./retryUtil";

const db = new ConnectionPool({
    user: config.get("connections.default.user"),
    password: config.get("connections.default.pwd"),
    server: config.get("connections.default.server"),
    database: config.get("connections.default.database"),
    connectionTimeout: config.get<number>("connections.default.connectionTimeout"),
    requestTimeout: config.get<number>("connections.default.requestTimeout"),
    options: {
        encrypt: config.get<boolean>("connections.default.encrypt") // azure sqldatabse 需要设置为 true
    },
    pool: {
        max: 64,
        min: 1,
        idleTimeoutMillis: 120000
    }
}, err => {
    // ... error handler
    if (err) {
        logger.error(err);
    }
});
db.on("error", err => {
    if (err) {
        logger.error(err);
    }
});

export class DbUtil {
    private static async ensureConnected(): Promise<void> {
        while (db.connecting) {
            await sleep(10);
        }
        if (db.connected) {
            return;
        }
        db.connect(err => {
            if (err) {
                logger.error(`数据库连接失败： ${err}`);
            }
        });
    }

    private static async executeSql(request: Request, sql: string, retryTimes: number = 3): Promise<IResult<any>> {
        return await RetryUtil.tryInvokeAsync(async () => {
            await this.ensureConnected();
            logger.debug(`sql to execute: ${sql}`);
            var result = await request.query(sql);
            logger.debug(`sql executed result, rowsAffected:${result.rowsAffected}`);
            await db.close(); // close connection
            return result;
        }, res => res != null, retryTimes, 1);
    }

    public static async query(sql: string, params?: any, retryTimes: number = 3): Promise<IResult<any>> { //写sql语句自由查询
        let request = db.request();
        if (params) {
            for (var index in params) {
                if (typeof params[index] == "number") {
                    request.input(index, mssql.Int, params[index]);
                } else if (typeof params[index] == "string") {
                    request.input(index, mssql.NVarChar, params[index]);
                } else if (typeof params[index] == "boolean") {
                    request.input(index, mssql.Bit, params[index]);
                }
            }
        }
        return await this.executeSql(request, sql, retryTimes);
    };

    public static async add(addObj: any, tableName: string): Promise<IResult<any> | null> {//添加数据
        if (!addObj) {
            return null;
        }
        var request = db.request();
        var sql = "insert into " + tableName + "(";
        for (var index in addObj) {
            if (!addObj.hasOwnProperty(index)) {
                continue;
            }
            if (typeof addObj[index] == "number") {
                request.input(index, mssql.BigInt, addObj[index]);
            } else if (typeof addObj[index] == "string") {
                request.input(index, mssql.NVarChar, addObj[index]);
            } else if (typeof addObj[index] == "boolean") {
                request.input(index, mssql.Bit, addObj[index]);
            } else if (addObj[index] instanceof Buffer) {
                request.input(index + 'Where', mssql.VarBinary, addObj[index]);
            } else {
                request.input(index, mssql.NVarChar, addObj[index] == null ? null : addObj[index].toString());
            }
            sql += index + ",";
        }
        sql = sql.substring(0, sql.length - 1) + ") values(";
        for (var index in addObj) {
            if (!addObj.hasOwnProperty(index)) {
                continue;
            }
            sql += "@" + index + ",";
        }
        sql = sql.substring(0, sql.length - 1) + ")";

        return await this.executeSql(request, sql);
    };

    public static async addIfNotExist(addObj: any, tableName: string, whereObj?: any): Promise<IResult<any> | null> {//添加数据
        if (!addObj) {
            return null;
        }
        if (whereObj == null) {
            return await this.add(addObj, tableName);
        }

        var request = db.request();
        let sql = `BEGIN
        IF NOT EXISTS (SELECT 1 FROM ${tableName} WHERE 1 > 0`;
        for (var index in whereObj) {
            if (typeof whereObj[index] == "number") {
                request.input(index + 'Where', mssql.Int, whereObj[index]);
            } else if (typeof whereObj[index] == "string") {
                request.input(index + 'Where', mssql.NVarChar, whereObj[index]);
            } else if (typeof whereObj[index] == "boolean") {
                request.input(index + 'Where', mssql.Bit, whereObj[index]);
            } else {
                request.input(index+"Where", mssql.NVarChar, whereObj[index] == null ? null : addObj[index].toString());
            }
            sql += ` AND ${index} = @${index}Where`
        }
        sql += ')';

        sql += 'BEGIN ';
        sql += "INSERT INTO " + tableName + "(";
        for (var index in addObj) {
            if (!addObj.hasOwnProperty(index)) {
                continue;
            }            
            console.log(`propName: ${index}, type: ${typeof addObj[index]}`);

            if (typeof addObj[index] == "number") {
                request.input(index, mssql.Int, addObj[index]);
            } else if (typeof addObj[index] == "string") {
                request.input(index, mssql.NVarChar, addObj[index]);
            } else if (typeof addObj[index] == "boolean") {
                request.input(index, mssql.Bit, addObj[index]);
            } else if (addObj[index] instanceof Buffer) {
                request.input(index, mssql.VarBinary, addObj[index]);
            } else {
                request.input(index, mssql.NVarChar, addObj[index] == null ? null : addObj[index].toString());
            }
            sql += index + ",";
        }
        sql = sql.substring(0, sql.length - 1) + ")values(";
        for (var index in addObj) {
            if (!addObj.hasOwnProperty(index)) {
                continue;
            }
            sql += `@${index},`;
        }
        sql = sql.substring(0, sql.length - 1) + ")";

        sql += `   END
        END`;

        return await this.executeSql(request, sql);
    };

    public static async addList(addObjs: Array<any>, tableName: string): Promise<IResult<any> | null> {//添加数据
        if (!addObjs || addObjs.length == 0) {
            return null;
        }
        var sql = "INSERT INTO " + tableName + "(";
        if (addObjs) {
            let addObj = addObjs[0];
            for (var index in addObj) {
                if (!addObj.hasOwnProperty(index)) {
                    continue;
                }
                sql += index + ",";
            }
            sql = sql.substring(0, sql.length - 1) + ") VALUES";
            addObjs.forEach(addObj => {
                sql = sql + "(";
                for (var index in addObj) {
                    if (!addObj.hasOwnProperty(index)) {
                        continue;
                    }
                    if (typeof addObj[index] == "number") {
                        sql += addObj[index] + ",";
                    } else if (typeof addObj[index] == "string") {
                        sql += "N'" + addObj[index] + "'" + ",";
                    } else if (typeof addObj[index] == "boolean") {
                        sql += (addObj[index] ? '1' : '0') + ",";
                    } else {
                        if(addObj[index] == null){
                            sql += `NULL,`;
                        }else{
                            sql += `${addObj[index]},`;
                        }                        
                    }
                }
                sql = sql.substring(0, sql.length - 1) + "),";
            });
        }
        sql = sql.substring(0, sql.length - 1);

        return await this.executeSql(db.request(), sql);
    };

    public static async addListIfNotExist(addObjs: Array<any>, uniqueFieldName: string, tableName: string): Promise<IResult<any> | null> {//添加数据
        if (!addObjs || addObjs.length == 0) {
            return null;
        }
        let addObj = addObjs[0];
        let sql = `
          CREATE TABLE #${tableName}Temp(
        `;

        for (var index in addObj) {
            if (!addObj.hasOwnProperty(index)) {
                continue;
            }
            if (typeof addObj[index] == "number") {
                sql += index + " BIGINT,";
            } else if (typeof addObj[index] == "string") {
                sql += index + " NVARCHAR(MAX),";
            } else if (typeof addObj[index] == "boolean") {
                sql += index + " BIT,";
            } else {
                sql += index + " NVARCHAR(MAX),";
            }
        }
        sql = sql.substring(0, sql.length - 1) + ");";

        sql += "INSERT INTO #" + tableName + "Temp(";
        if (addObjs) {
            for (var index in addObj) {
                if (!addObj.hasOwnProperty(index)) {
                    continue;
                }
                sql += index + ",";
            }
            sql = sql.substring(0, sql.length - 1) + ") VALUES";
            addObjs.forEach(addObj => {
                sql = sql + "(";
                for (var index in addObj) {
                    if (!addObj.hasOwnProperty(index)) {
                        continue;
                    }
                    if (typeof addObj[index] == "number") {
                        sql += addObj[index] + ",";
                    } else if (typeof addObj[index] == "string") {
                        sql += "N'" + addObj[index] + "'" + ",";
                    } else if (typeof addObj[index] == "boolean") {
                        sql += (addObj[index] ? "1" : "0") + ",";
                    } else {
                        if(addObj[index] == null){
                            sql += `NULL,`;
                        }else{
                            sql += `${addObj[index]},`;
                        }  
                    }
                }
                sql = sql.substring(0, sql.length - 1) + "),";
            });
        }
        sql = sql.substring(0, sql.length - 1) + ";";
        //
        sql += "INSERT INTO " + tableName + "(";
        for (var index in addObj) {
            if (!addObj.hasOwnProperty(index)) {
                continue;
            }
            sql += index + ",";
        }
        sql = sql.substring(0, sql.length - 1) + ") SELECT";
        for (var index in addObj) {
            if (!addObj.hasOwnProperty(index)) {
                continue;
            }
            sql += index + ",";
        }
        sql = sql.substring(0, sql.length - 1);
        sql += ` FROM #${tableName}Temp
        WHERE ${uniqueFieldName} NOT IN (
            SELECT ${uniqueFieldName} FROM ${tableName}
        )
        `;

        return await this.executeSql(db.request(), sql);
    };

    public static async update(updateObj: any, whereObj: any, tableName: string): Promise<IResult<any>> {//更新数据
        var request = db.request();

        var sql = "UPDATE " + tableName + " SET ";
        if (updateObj) {
            for (var index in updateObj) {
                if (!updateObj.hasOwnProperty(index)) {
                    continue;
                }
                if (typeof updateObj[index] == "number") {
                    request.input(index, mssql.Int, updateObj[index]);
                } else if (typeof updateObj[index] == "string") {
                    request.input(index, mssql.NVarChar, updateObj[index]);
                } else if (typeof updateObj[index] == "boolean") {
                    request.input(index, mssql.Bit, updateObj[index]);
                } else if (updateObj[index] instanceof Buffer) {
                    request.input(index + 'Where', mssql.VarBinary, updateObj[index]);
                } 
                sql += index + "=@" + index + ",";
            }
        }
        sql = sql.substring(0, sql.length - 1) + " WHERE ";
        if (whereObj) {
            for (var index in whereObj) {
                if (typeof whereObj[index] == "number") {
                    request.input(index, mssql.Int, whereObj[index]);
                } else if (typeof whereObj[index] == "string") {
                    request.input(index, mssql.NVarChar, whereObj[index]);
                } else if (typeof whereObj[index] == "boolean") {
                    request.input(index, mssql.Bit, whereObj[index]);
                } else if (whereObj[index] instanceof Buffer) {
                    request.input(index, mssql.VarBinary, whereObj[index]);
                } else {
                    request.input(index, mssql.NVarChar, whereObj[index] == null ? null : whereObj[index].toString());
                }
                sql += index + "=@" + index + " AND ";
            }
        }
        sql = sql.substring(0, sql.length - 5);

        return await this.executeSql(request, sql);
    };
}