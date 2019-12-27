const log4js = require('log4js');
const dbUtil = require('./dataaccess/dbUtil');

// config log4js
log4js.configure({
    appenders: {
      everything: { type: 'file', filename: './logs/sys.log', maxLogSize: 2048000 }
    },
    categories: {
      default: { appenders: [ 'everything' ], level: 'info' }
    }
  });

(async()=>{
    await dbUtil.add({
        Extra: '{}',
        CreatedAt: '2019-12-10 10:20:00'
    }, 'TestEntities');

    let results = await dbUtil.query(`SELECT * FROM TestEntities`)
    console.log(JSON.stringify(results));
})();