using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hitcon2018.Wallet
{    
    public class AccountDAO
    {
        readonly SQLiteAsyncConnection database;
        public AccountDAO(SQLiteAsyncConnection _database)
        {
            database = _database;
        }

        public Task<List<AccountSetting>> GetItemsAsync()
        {
            return database.Table<AccountSetting>().ToListAsync();
        }

        public Task<AccountSetting> GetItemAsync(string address)
        {
            return database.Table<AccountSetting>().Where(i => i.Address == address).FirstOrDefaultAsync();
        }

        public Task<int> InsertItemAsync(AccountSetting item)
        {    
            return database.InsertAsync(item);   
        }

        public Task<int> UpdateItemAsync(AccountSetting item)
        {
            return database.UpdateAsync(item);
        }

        public Task<int> DeleteItemAsync(AccountSetting item)
        {
            return database.DeleteAsync(item);
        }

        public Task<int> DeleteItemsAync()
        {
            return database.DropTableAsync<AccountSetting>();
        }
    }
}
