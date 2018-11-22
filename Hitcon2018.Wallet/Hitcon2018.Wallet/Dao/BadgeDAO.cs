using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hitcon2018.Wallet
{
    public class BadgeDAO
    {
        readonly SQLiteAsyncConnection database;
        public BadgeDAO(SQLiteAsyncConnection _database)
        {
            database = _database;
        }

        public Task<List<BadgeSetting>> GetItemsAsync()
        {
            return database.Table<BadgeSetting>().ToListAsync();
        }

        //public Task<List<BadgeSetting>> GetItemsNotDoneAsync()
        //{
        //    return database.QueryAsync<BadgeSetting>("SELECT * FROM [TodoItem] WHERE [Done] = 0");
        //}

        public Task<BadgeSetting> GetItemAsync(int id)
        {
            return database.Table<BadgeSetting>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public Task<int> SaveItemAsync(BadgeSetting item)
        {
            if (item.ID != 0)
            {
                return database.UpdateAsync(item);
            }
            else
            {
                return database.InsertAsync(item);
            }
        }

        public Task<int> DeleteItemAsync(BadgeSetting item)
        {
            return database.DeleteAsync(item);
        }

        public Task<int> DeleteItemsAync()
        {
            return database.DropTableAsync<BadgeSetting>();
        }
    }
}
