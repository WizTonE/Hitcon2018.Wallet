using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hitcon2018.Wallet
{
    public class WalletDatabase
    {
        readonly SQLiteAsyncConnection database;
        public BadgeDAO BadgeDAO { get; set; }
        public AccountDAO AccountDAO { get; set; }
        public TransactionDAO TransactionDAO { get; set; }
        public WalletDatabase(string dbPath)
        {
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<BadgeSetting>().Wait();
            database.CreateTableAsync<AccountSetting>().Wait();
            database.CreateTableAsync<Transaction>().Wait();
            BadgeDAO = new BadgeDAO(database);
            AccountDAO = new AccountDAO(database);
            TransactionDAO = new TransactionDAO(database);
        }

        public void ReBuildDataBase()
        {
            database.CreateTableAsync<BadgeSetting>().Wait();
            database.CreateTableAsync<AccountSetting>().Wait();
            database.CreateTableAsync<Transaction>().Wait();
        }

        //public Task<List<BadgeSetting>> GetItemsAsync()
        //{
        //    return database.Table<BadgeSetting>().ToListAsync();
        //}

        //public Task<List<BadgeSetting>> GetItemsNotDoneAsync()
        //{
        //    return database.QueryAsync<BadgeSetting>("SELECT * FROM [TodoItem] WHERE [Done] = 0");
        //}

        //public Task<BadgeSetting> GetItemAsync(int id)
        //{
        //    return database.Table<BadgeSetting>().Where(i => i.ID == id).FirstOrDefaultAsync();
        //}

        //public Task<int> SaveItemAsync(BadgeSetting item)
        //{
        //    if (item.ID != 0)
        //    {
        //        return database.UpdateAsync(item);
        //    }
        //    else
        //    {
        //        return database.InsertAsync(item);
        //    }
        //}

        //public Task<int> DeleteItemAsync(BadgeSetting item)
        //{
        //    return database.DeleteAsync(item);
        //}
    }
}