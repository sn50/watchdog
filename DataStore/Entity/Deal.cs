using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataStore.Entity
{
    /// <summary>
    /// Represents FOREX deal made by a customer on a server.
    /// </summary>
    public class Deal
    {
        public long Id { get; set; }

        #region References to other entities

        public int UserId { get; set; }
        
        /// <summary>
        /// User who made this deal.
        /// </summary>
        public virtual User User { get; set; }

        public int ServerId { get; set; }

        /// <summary>
        /// Server the deal was executed at.
        /// </summary>
        public virtual Server Server { get; set; }

        public int SymbolId { get; set; }

        /// <summary>
        /// Symbol.
        /// </summary>
        public virtual Symbol Symbol { get; set; } 
        
        public int? GroupId { get; set; }

        /// <summary>
        /// Group of deals, if the deal belongs to any.
        /// </summary>
        public virtual DealGroup Group { get; set; }

        #endregion

        /// <summary>
        /// Deal ID from trading server.
        /// </summary>
        public long ExternalId { get; set; }

        /// <summary>
        /// Deal type / BUY or SEL
        /// </summary>
        public DealType Type { get; set; }

        /// <summary>
        /// Volume in 1/10000 lots.
        /// </summary>
        /// <example>1 lot: <c>Volume==10000</c></example>
        public long Volume { get; set; }

        /// <summary>
        /// Date and time (local) the deal was executed.
        /// </summary>
        [Index]
        public DateTime Date { get; set; }
        
        #region Post-processed values
        
        /// <summary>
        /// User balance at the time of the deal arrival event.
        /// </summary>
        public decimal? Balance { get; set; }

        /// <summary>
        /// Metric computed from <see cref="Volume"/>
        /// and <see cref="Balance"/>. Ratio between
        /// the the deal volume and current user balance.
        /// </summary>
        public decimal? VolumeToBalanceRatio { get; set; } 
        
        #endregion

        public override string ToString()
        {
            return $"ID #{Id}, External: #{ExternalId}, Balance: {Balance}, {Type} {Symbol?.Name} {Volume/10000.0} lot(s) at {Date:G} (GroupID: #{GroupId}, User {User?.Login}, VTBRatio:{VolumeToBalanceRatio})";
        }
    }
}
