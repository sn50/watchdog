using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchdogFramework.Configuration
{
    public sealed class ServerInstanceConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get => (string)base["name"];
            set => base["name"] = value;
        }

        [ConfigurationProperty("address", IsRequired = true)]
        public string Address
        {
            get => (string)base["address"];
            set => base["address"] = value;
        }

        [ConfigurationProperty("login", IsRequired = true)]
        public long Login
        {
            get => (long)base["login"];
            set => base["login"] = value;
        }
        
        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get => (string)base["password"];
            set => base["password"] = value;
        }
    }

    public sealed class ServerInstanceConfigurationElementCollection : ConfigurationElementCollection
    {
        public ServerInstanceConfigurationElement this[int index]
        {
            get => (ServerInstanceConfigurationElement)BaseGet(index);
            set
            {
                if (BaseGet(index) != null) 
                    BaseRemoveAt(index);

                BaseAdd(index, value);
            }
        }
        
        public ServerInstanceConfigurationElement this[string key]
        {
            get => (ServerInstanceConfigurationElement)BaseGet(key);
            set
            {
                if (BaseGet(key) != null) 
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));

                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerInstanceConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServerInstanceConfigurationElement)element).Name;
        }
    }

    public sealed class ServerInstancesConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("servers")]
        [ConfigurationCollection(typeof(ServerInstanceConfigurationElementCollection))]
        public ServerInstanceConfigurationElementCollection ServerInstances => (ServerInstanceConfigurationElementCollection)this["servers"];
    }
}
