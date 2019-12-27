import { configure, getLogger, connectLogger } from "log4js";
import * as fs from 'fs';
import { DbUtil } from './utils/dbUtil';

// config log4js
configure({
  appenders: {
    file: {
      type: 'file',
      filename: './logs/sys.log',
      maxLogSize: 1024000,
      backups: 10
    },
    errorFile: {
      type: "file",
      filename: "./logs/errors.log"
    },
    errors: {
      type: "logLevelFilter",
      level: "ERROR",
      appender: "errorFile"
    },
    httpLog: {
      type: 'file',
      filename: './logs/http.log',
      maxLogSize: 204800,
      backups: 3
    },
    dbUtil: {
      type: 'file',
      filename: './logs/dbUtil.log',
      maxLogSize: 1024000,
      backups: 3
    },
    console: { type: 'console' }
  },
  categories: {
    default: { appenders: ['file', 'console', 'errors'], level: 'debug' },
    dbUtil: { appenders: ['dbUtil'], level: 'debug' },
    http: { appenders: ['httpLog'], level: 'debug' }
  }
});
(async ()=>{  
  await DbUtil.add({
    Extra: '{}',
    CreatedAt: '2019-12-10 10:20:00'
  }, 'TestEntities');
  let results = await DbUtil.query(`SELECT * FROM TestEntities`)
  console.log(JSON.stringify(results));
})();
