/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements.  See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership.  The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License.  You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/


using System.Collections.ObjectModel;

namespace ThinkGeo.MapSuite.GisEditor.Plugins
{
    public class GetMsSqlDatabasesParameter
    {
        private Collection<string> databaseNames;
        private MsSqlServerDataRepositoryItem serverItem;
        private MsSql2008FeatureLayerInfo msSqllayerInfo;

        public GetMsSqlDatabasesParameter()
        {
            databaseNames = new Collection<string>();
        }

        public MsSql2008FeatureLayerInfo MsSqllayerInfo
        {
            get { return msSqllayerInfo; }
            set { msSqllayerInfo = value; }
        }

        public Collection<string> DatabaseNames
        {
            get { return databaseNames; }
        }

        public MsSqlServerDataRepositoryItem ServerItem
        {
            get { return serverItem; }
            set { serverItem = value; }
        }
    }
}