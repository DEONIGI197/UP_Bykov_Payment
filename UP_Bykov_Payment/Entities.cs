namespace UP_Bykov_Payment
{
    // подключение к базе данных
    public partial class DB_Bykov_PaymentEntities
    {
        private static DB_Bykov_PaymentEntities _context;

        public static DB_Bykov_PaymentEntities GetContext()
        {
            if (_context == null)
                _context = new DB_Bykov_PaymentEntities();
            return _context;
        }
    }
}
