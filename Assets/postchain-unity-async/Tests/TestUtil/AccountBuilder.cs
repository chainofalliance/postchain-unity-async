using Chromia.Postchain.Ft3;

using System.Collections.Generic;
using System.Linq;
using System;

using Chromia.Postchain.Client;
using Cysharp.Threading.Tasks;

public class AccountBuilder
{
    private Blockchain _blockchain;
    private User _user;
    private int _balance = -1;
    private Asset _asset;
    private List<KeyPair> _participants = new List<KeyPair>() { new KeyPair() };
    private int _requiredSignaturesCount = 1;
    private List<FlagsType> _flags = new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer };
    private int _points = 0;

    public AccountBuilder(Blockchain blockchain, User user)
    {
        User _user = user;
        if (user == null)
        {
            _user = TestUser.SingleSig();
        }

        this._blockchain = blockchain;
        this._participants = new List<KeyPair>() { _user.KeyPair };
        this._user = _user;
    }

    #region public
    public static AccountBuilder CreateAccountBuilder(Blockchain blockchain, User user = null)
    {
        return new AccountBuilder(blockchain, user);
    }

    public AccountBuilder WithAuthFlags(IEnumerable<FlagsType> flags)
    {
        this._flags = flags.ToList();
        return this;
    }
    public AccountBuilder WithParticipants(IEnumerable<KeyPair> participants)
    {
        this._participants = participants.ToList();
        return this;
    }

    public AccountBuilder WithBalance(Asset asset, int balance)
    {
        this._asset = asset;
        this._balance = balance;
        return this;
    }

    public AccountBuilder WithPoints(int points)
    {
        this._points = points;
        return this;
    }

    public AccountBuilder WithRequiredSignatures(int count)
    {
        this._requiredSignaturesCount = count;
        return this;
    }

    public async UniTask<PostchainResponse<Account>> Build()
    {
        var res = await this.RegisterAccount();

        if (res.Error)
            return res;

        if (!res.Error)
        {
            await this.AddBalanceIfNeeded(res.Content);
            var pointRes = await this.AddPointsIfNeeded(res.Content);

            if (!pointRes.Error)
                res.Content.RateLimit = pointRes.Content;
        }

        return res;
    }
    #endregion

    #region private

    private UniTask<PostchainResponse<Account>> RegisterAccount()
    {
        return Account.Register(
            this.GetAuthDescriptor(),
            this._blockchain.NewSession(this._user)
        );
    }

    private async UniTask<PostchainResponse<string>> AddBalanceIfNeeded(Account account)
    {
        if (this._asset != null && this._balance != -1)
        {
            return await AssetBalance.GiveBalance(account.Id, this._asset.Id, this._balance, this._blockchain);
        }

        return PostchainResponse<string>.ErrorResponse("Asset not valid");
    }

    private async UniTask<PostchainResponse<RateLimit>> AddPointsIfNeeded(Account account)
    {
        if (this._points > 0)
        {
            await RateLimit.GivePoints(account.Id, this._points, this._blockchain);
        }

        return await RateLimit.GetByAccountRateLimit(account.Id, this._blockchain);
    }

    private AuthDescriptor GetAuthDescriptor()
    {
        if (this._requiredSignaturesCount > this._participants.Count)
        {
            throw new Exception("Number of required signatures has to be less than number of participants");
        }

        if (this._participants.Count > 1)
        {
            var participants = new List<byte[]>();
            foreach (var participant in this._participants)
            {
                participants.Add(participant.PubKey);
            }

            return new MultiSignatureAuthDescriptor(
                participants,
                this._requiredSignaturesCount,
                this._flags.ToArray(),
                this._user.AuthDescriptor.Rule
            );
        }
        else
        {
            return new SingleSignatureAuthDescriptor(
                this._participants[0].PubKey,
                this._flags.ToArray(),
                this._user.AuthDescriptor.Rule
                );
        }
    }
    #endregion
}
