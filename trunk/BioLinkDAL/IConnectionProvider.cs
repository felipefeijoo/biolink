using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using BioLink.Data.Model;

namespace BioLink.Data {

    public interface IConnectionProvider {
        DbConnection GetConnection(ConnectionProfile profile, String username, String password);
        void StoredProcReaderFirst(User user, DbCommand command, string proc, ServiceReaderAction action, Action<String> message, params DbParameter[] @params);
        void StoredProcReaderForEach(User user, DbCommand command, string proc, ServiceReaderAction action, Action<String> message, params DbParameter[] @params);
        int StoredProcUpdate(User user, DbCommand command, string proc, params DbParameter[] @params);
        bool IsSysAdmin(User user);

    }
}
