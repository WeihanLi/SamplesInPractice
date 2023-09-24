using dotenv.net;

// load .env variables
DotEnv.Load();

await MemorySample.MainTest();
