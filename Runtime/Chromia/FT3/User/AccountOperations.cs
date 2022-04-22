using System;

namespace Chromia.Postchain.Ft3
{
    public static class AccountOperations
    {
        public static Operation AddAuthDescriptor(string accountId, string authDescriptorId, AuthDescriptor authDescriptor)
        {
            return new Operation("ft3.add_auth_descriptor", new object[] { accountId, authDescriptorId, authDescriptor.ToGTV() });
        }

        public static Operation Transfer(object[] inputs, object[] outputs)
        {
            return new Operation("ft3.transfer", new object[] { inputs, outputs });
        }

        public static Operation XcTransfer(object[] source, object[] target, string[] hops)
        {
            return new Operation("ft3.xc.init_xfer", new object[] { source, target, hops });
        }

        public static Operation DeleteAllAuthDescriptorsExclude(string accountId, string excludeAuthDescriptorId)
        {
            return new Operation("ft3.delete_all_auth_descriptors_exclude", new object[] { accountId, excludeAuthDescriptorId });
        }

        public static Operation DeleteAuthDescriptor(string accountId, string authDescriptorId, string deleteAuthDescriptorId)
        {
            return new Operation("ft3.delete_auth_descriptor", new object[] { accountId, authDescriptorId, deleteAuthDescriptorId });
        }

        public static Operation Nop()
        {
            return new Operation("nop", new object[] { new Random().Next().ToString() });
        }

        public static Operation Op(string name, object[] args)
        {
            return new Operation(name, args);
        }
    }
}