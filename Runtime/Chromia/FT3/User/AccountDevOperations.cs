namespace Chromia.Postchain.Ft3
{
    public static class AccountDevOperations
    {
        public static Operation Register(AuthDescriptor authDescriptor)
        {
            return new Operation("ft3.dev_register_account", new object[] { authDescriptor.ToGTV() });
        }

        public static Operation FreeOp(string accountID)
        {
            return new Operation("ft3.dev_free_op", new object[] { accountID });
        }

        public static Operation GivePoints(string accountID, int points)
        {
            return new Operation("ft3.dev_give_points", new object[] { accountID, points });
        }
    }
}