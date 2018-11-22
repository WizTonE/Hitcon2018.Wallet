using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hitcon2018.Wallet
{
    public class TransactionDAO
    {
        readonly SQLiteAsyncConnection database;
        public TransactionDAO(SQLiteAsyncConnection _database)
        {
            database = _database;
        }

        public Task<List<Transaction>> GetItemsAsync()
        {
            return database.Table<Transaction>().ToListAsync();
        }

        public Task<Transaction> GetItemAsync(string address)
        {
            return database.Table<Transaction>().Where(i => i.Address == address).FirstOrDefaultAsync();
        }

        public Task<int> SaveItemAsync(Transaction item)
        {
            if (!string.IsNullOrEmpty(item.Address))
            {
                return database.UpdateAsync(item);
            }
            else
            {
                return database.InsertAsync(item);
            }
        }

        public Task<int> DeleteItemAsync(Transaction item)
        {
            return database.DeleteAsync(item);
        }

        public Task<int> DeleteItemsAync()
        {
            return database.DropTableAsync<Transaction>();
        }
    }
}
