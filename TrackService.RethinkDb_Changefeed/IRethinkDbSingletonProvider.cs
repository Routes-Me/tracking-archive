using RethinkDb.Driver;
using RethinkDb.Driver.Net;

namespace TrackService.RethinkDb_Changefeed
{
    internal interface IRethinkDbSingletonProvider
    {
        RethinkDB RethinkDbSingleton { get; }

        Connection RethinkDbConnection { get; }
    }
}
