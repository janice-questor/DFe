﻿#pragma warning disable CS1591

#if INTEROP
using System.Runtime.InteropServices;
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;
using SystemUnimake;

namespace Unimake.Business.DFe.Xml.NFe
{
#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.EnvEvento")]
    [ComVisible(true)]
#endif
    [Serializable()]
    [XmlRoot("envEvento", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class EnvEvento : XMLBase
    {
        private void SignEvent(Evento evento, XmlElement xmlEl)
        {
            var signature = xmlEl.GetElementsByTagName("Signature")[0];
            if (signature != null)
            {
                var signatureEvento = new XmlDocument();

                signatureEvento.LoadXml(signature.OuterXml);
                evento.Signature = XMLUtility.Deserializar<Signature>(signatureEvento);
            }
        }

        [XmlElement("evento", Order = 2)]
        public List<Evento> Evento { get; set; } = new List<Evento>();

        [XmlElement("idLote", Order = 1)]
        public string IdLote { get; set; }

        [XmlAttribute(AttributeName = "versao", DataType = "token")]
        public string Versao { get; set; }

        public override void ReadXml(XmlDocument document)
        {
            base.ReadXml(document);

            switch (document.GetElementsByTagName("tpEvento")[0].InnerText)
            {
                case "111500":
                case "111501":
                    PreparaItemPedido(document);
                    break;

                case "110750":
                    PreparaDetPag(document);
                    break;
            }
        }

        public void PreparaDetPag(XmlDocument xmlDoc)
        {
            var detPags = xmlDoc.GetElementsByTagName("detPag");

            foreach (var evento in Evento)
            {
                if (evento.InfEvento.DetEvento is DetEventoConciliacaoFinanceira detEvento)
                {
                    detEvento.DetPag = new List<DetPagECONF>();

                    foreach (var nodeDetPag in detPags)
                    {
                        var elementDetPag = (XmlElement)nodeDetPag;

                        detEvento.DetPag.Add(new DetPagECONF
                        {
#if INTEROP
                            IndPag = elementDetPag.GetElementsByTagName("indPag").Count > 0 ? (IndicadorPagamento)Convert.ToInt32(elementDetPag.GetElementsByTagName("indPag")[0].InnerText) : (IndicadorPagamento)(-1),
#else
                            IndPag = elementDetPag.GetElementsByTagName("indPag").Count > 0 ? (IndicadorPagamento?)Convert.ToInt32(elementDetPag.GetElementsByTagName("indPag")[0].InnerText) : null,
#endif
                            TPag = (MeioPagamento)Convert.ToInt32(elementDetPag.GetElementsByTagName("tPag")[0].InnerText),
                            XPag = elementDetPag.GetElementsByTagName("xPag").Count > 0 ? elementDetPag.GetElementsByTagName("xPag")[0].InnerText : "",
                            VPag = Convert.ToDouble(elementDetPag.GetElementsByTagName("vPag")[0].InnerText, CultureInfo.InvariantCulture),
                            DPag = Convert.ToDateTime(elementDetPag.GetElementsByTagName("dPag")[0].InnerText, CultureInfo.InvariantCulture),
                            CNPJPag = elementDetPag.GetElementsByTagName("CNPJPag").Count > 0 ? elementDetPag.GetElementsByTagName("CNPJPag")[0].InnerText : "",
                            UFPag = elementDetPag.GetElementsByTagName("CNPJPag").Count > 0 ? (UFBrasil)Enum.Parse(typeof(UFBrasil), elementDetPag.GetElementsByTagName("UFPag")[0].InnerText) : UFBrasil.AN,
                            CNPJIF = elementDetPag.GetElementsByTagName("CNPJIF").Count > 0 ? elementDetPag.GetElementsByTagName("CNPJIF")[0].InnerText : "",
#if INTEROP
                            TBand = elementDetPag.GetElementsByTagName("tBand").Count > 0 ? (BandeiraOperadoraCartao)Convert.ToInt32(elementDetPag.GetElementsByTagName("tBand")[0].InnerText) : (BandeiraOperadoraCartao)(-1),
#else
                            TBand = elementDetPag.GetElementsByTagName("tBand").Count > 0 ? (BandeiraOperadoraCartao?)Convert.ToInt32(elementDetPag.GetElementsByTagName("tBand")[0].InnerText) : null,
#endif
                            CAut = elementDetPag.GetElementsByTagName("cAut").Count > 0 ? elementDetPag.GetElementsByTagName("cAut")[0].InnerText : "",
                            CNPJReceb = elementDetPag.GetElementsByTagName("CNPJReceb").Count > 0 ? elementDetPag.GetElementsByTagName("CNPJReceb")[0].InnerText : "",
                            UFReceb = elementDetPag.GetElementsByTagName("CNPJReceb").Count > 0 ? (UFBrasil)Enum.Parse(typeof(UFBrasil), elementDetPag.GetElementsByTagName("UFReceb")[0].InnerText) : UFBrasil.AN,
                        });
                    }
                }
            }
        }

        public void PreparaItemPedido(XmlDocument xmlDoc)
        {
            var itensPedidos = xmlDoc.GetElementsByTagName("itemPedido");

            foreach (var evento in Evento)
            {
                if (evento.InfEvento.DetEvento is DetEventoPedidoProrrogPrazoICMS detEvento)
                {
                    detEvento.ItemPedido = new List<ItemPedidoProrrogPrazoICMS>();

                    foreach (var nodeItemPedido in itensPedidos)
                    {
                        var elementItemPedido = (XmlElement)nodeItemPedido;

                        detEvento.ItemPedido.Add(new ItemPedidoProrrogPrazoICMS
                        {
                            NumItem = Convert.ToInt32(elementItemPedido.GetAttribute("numItem")),
                            QtdeItem = Convert.ToDouble(elementItemPedido.GetElementsByTagName("qtdeItem")[0].InnerText, CultureInfo.InvariantCulture)
                        });
                    }
                }
            }
        }

#if INTEROP
        /// <summary>
        /// Adicionar novo elemento a lista
        /// </summary>
        /// <param name="evento">Elemento</param>
        public void AddEvento(Evento evento)
        {
            if (Evento == null)
            {
                Evento = new List<Evento>();
            }

            Evento.Add(evento);
        }

        /// <summary>
        /// Retorna o elemento da lista Evento (Utilizado para linguagens diferentes do CSharp que não conseguem pegar o conteúdo da lista)
        /// </summary>
        /// <param name="index">Índice da lista a ser retornado (Começa com 0 (zero))</param>
        /// <returns>Conteúdo do index passado por parâmetro da Evento</returns>
        public Evento GetEvento(int index)
        {
            if ((Evento?.Count ?? 0) == 0)
            {
                return default;
            };

            return Evento[index];
        }

        /// <summary>
        /// Retorna a quantidade de elementos existentes na lista Evento
        /// </summary>
        public int GetEventoCount => (Evento != null ? Evento.Count : 0);
#endif

        public override XmlDocument GerarXML()
        {
            var xmlDocument = base.GerarXML();

            #region Adicionar o atributo de namespace que falta nas tags "evento"

            var attribute = GetType().GetCustomAttribute<XmlRootAttribute>();

            for (var i = 0; i < xmlDocument.GetElementsByTagName("evento").Count; i++)
            {
                var xmlElement = (XmlElement)xmlDocument.GetElementsByTagName("evento")[i];
                xmlElement.SetAttribute("xmlns", attribute.Namespace);
            }

            #endregion Adicionar o atributo de namespace que falta nas tags "evento"

            return xmlDocument;
        }

        public override T LerXML<T>(XmlDocument doc)
        {
            if (typeof(T) != typeof(EnvEvento))
            {
                throw new InvalidCastException($"Cannot cast type '{typeof(T).Name}' into type '{typeof(EnvEvento).Name}'.");
            }

            var retornar = base.LerXML<T>(doc) as EnvEvento;

            var eventos = doc.GetElementsByTagName("evento");

            if ((eventos?.Count ?? 0) > 0)
            {
                retornar.Evento = new List<Evento>();

                foreach (XmlElement xmlEl in eventos)
                {
                    var xml = new StringBuilder();
                    xml.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    xml.Append($"<envEvento xmlns=\"{xmlEl.NamespaceURI}\">");
                    xml.Append($"{xmlEl.OuterXml}</envEvento>");

                    var envEvt = XMLUtility.Deserializar<EnvEvento>(xml.ToString());
                    var evt = envEvt.Evento[0];
                    SignEvent(evt, xmlEl);
                    retornar.Evento.Add(evt);
                }
            }

            return (T)(object)retornar;
        }

        /// <summary>
        /// Desserializar o XML no objeto EnvEvento
        /// </summary>
        /// <param name="filename">Localização do arquivo XML do envEvento</param>
        /// <returns>Objeto do EnvEvento</returns>
        public EnvEvento LoadFromFile(string filename)
        {
            var doc = new XmlDocument();
            doc.LoadXml(System.IO.File.ReadAllText(filename, Encoding.UTF8));
            return XMLUtility.Deserializar<EnvEvento>(doc);
        }

        /// <summary>
        /// Desserializar o XML envEvento no objeto EnvEvento
        /// </summary>
        /// <param name="xml">string do XML envEvento</param>
        /// <returns>Objeto da EnvEvento</returns>
        public EnvEvento LoadFromXML(string xml) => XMLUtility.Deserializar<EnvEvento>(xml);
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.Evento")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlType(Namespace = "http://www.portalfiscal.inf.br/nfe")]
    [XmlRoot("evento", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Evento : XMLBase
    {
        [XmlElement("infEvento", Order = 0)]
        public InfEvento InfEvento { get; set; }

        [XmlElement("Signature", Namespace = "http://www.w3.org/2000/09/xmldsig#", Order = 1)]
        public Signature Signature { get; set; }

        [XmlAttribute(AttributeName = "versao", DataType = "token")]
        public string Versao { get; set; }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.InfEvento")]
    [ComVisible(true)]
#endif
    [Serializable()]
    [XmlType(AnonymousType = true, Namespace = "http://www.portalfiscal.inf.br/nfe")]
    public class InfEvento
    {
        #region Private Fields

        private EventoDetalhe _detEvento;

        #endregion Private Fields

        #region Public Properties

        [XmlElement("chNFe", Order = 4)]
        public string ChNFe { get; set; }

        [XmlElement("CNPJ", Order = 2)]
        public string CNPJ { get; set; }

        [XmlIgnore]
        public UFBrasil COrgao { get; set; }

        [XmlElement("cOrgao", Order = 0)]
        public int COrgaoField
        {
            get => (int)COrgao;
            set => COrgao = (UFBrasil)Enum.Parse(typeof(UFBrasil), value.ToString());
        }

        [XmlElement("CPF", Order = 3)]
        public string CPF { get; set; }

        [XmlElement("detEvento", Order = 9)]
        public EventoDetalhe DetEvento
        {
            get => _detEvento;
            set
            {
                switch (TpEvento)
                {
                    case 0:
                        _detEvento = value;
                        break;

                    case TipoEventoNFe.CartaCorrecao:
                        _detEvento = new DetEventoCCE();
                        break;

                    case TipoEventoNFe.Cancelamento:
                        _detEvento = new DetEventoCanc();
                        break;

                    case TipoEventoNFe.ManifestacaoConfirmacaoOperacao:
                    case TipoEventoNFe.ManifestacaoCienciaOperacao:
                    case TipoEventoNFe.ManifestacaoDesconhecimentoOperacao:
                    case TipoEventoNFe.ManifestacaoOperacaoNaoRealizada:
                        _detEvento = new DetEventoManif();
                        break;

                    case TipoEventoNFe.AtorInteressadoNFe:
                        COrgao = UFBrasil.AN; //Sempre será 91, somente o ambiente nacional vai autorizar este evento
                        _detEvento = new DetEventoAtorInteressadoNFe();
                        break;

                    case TipoEventoNFe.CancelamentoPorSubstituicao:
                        _detEvento = new DetEventoCancSubst();
                        break;

                    case TipoEventoNFe.EPEC:
                        _detEvento = new DetEventoEPEC();
                        break;

                    case TipoEventoNFe.ComprovanteEntregaNFe:
                        _detEvento = new DetEventoCompEntregaNFe();
                        break;

                    case TipoEventoNFe.CancelamentoComprovanteEntregaNFe:
                        _detEvento = new DetEventoCancCompEntregaNFe();
                        break;

                    case TipoEventoNFe.PedidoProrrogacaoPrazo1:
                    case TipoEventoNFe.PedidoProrrogacaoPrazo2:
                        _detEvento = new DetEventoPedidoProrrogPrazoICMS();
                        break;

                    case TipoEventoNFe.CancelamentoPedidoProrrogacaoPrazo1:
                    case TipoEventoNFe.CancelamentoPedidoProrrogacaoPrazo2:
                        _detEvento = new DetEventoCancPedidoProrrogPrazoICMS();
                        break;

                    case TipoEventoNFe.CTeAutorizado:
                        _detEvento = new DetEventoCTeAutorizado();
                        break;

                    case TipoEventoNFe.MDFeAutorizadoComCTe:
                        _detEvento = new DetEventoMDFeAutorizadoComCTe();
                        break;

                    case TipoEventoNFe.ComprovantedeEntregaCTe:
                        _detEvento = new DetEventoComprovanteEntregaCTe();
                        break;

                    case TipoEventoNFe.CancelamentoComprovantedeEntregaCTe:
                        _detEvento = new DetEventoCancelamentoComprovanteEntregaCTe();
                        break;

                    case TipoEventoNFe.AverbacaoDeExportacao:
                        _detEvento = new DetEventoAverbacaoExportacao();
                        break;

                    case TipoEventoNFe.VistoriaSUFRAMASEFAZ:
                        _detEvento = new DetEventoVistoriaSuframaSEFAZ();
                        break;

                    case TipoEventoNFe.VistoriaSUFRAMA:
                        _detEvento = new DetEventoVistoriaSuframa();
                        break;

                    case TipoEventoNFe.InternalizacaoSUFRAMA:
                        _detEvento = new DetEventoInternalizacaoSUFRAMA();
                        break;

                    case TipoEventoNFe.InsucessoEntregaNFe:
                        COrgao = UFBrasil.SVRS; //Sempre será 92 no caso de Insucesso da Entrega da NFe, somente SVRS vai autorizar este evento.
                        _detEvento = new DetEventoInsucessoEntregaNFe();
                        break;

                    case TipoEventoNFe.CancelamentoInsucessoEntregaNFe:
                        COrgao = UFBrasil.SVRS; //Sempre será 92 no caso de Insucesso da Entrega da NFe, somente SVRS vai autorizar este evento.
                        _detEvento = new DetEventoCancelamentoInsucessoEntregaNFe();
                        break;

                    case TipoEventoNFe.ConciliacaoFinanceira:
                        _detEvento = new DetEventoConciliacaoFinanceira();
                        break;

                    case TipoEventoNFe.CancelamentoConciliacaoFinanceira:
                        _detEvento = new DetEventoCancelamentoConciliacaoFinanceira();
                        break;

                    default:
                        throw new NotImplementedException($"O tipo de evento '{TpEvento}' não está implementado.");
                }

                _detEvento.XmlReader = value.XmlReader;
                _detEvento.ProcessReader();
            }
        }

        [XmlIgnore]
#if INTEROP
        public DateTime DhEvento { get; set; }
#else
        public DateTimeOffset DhEvento { get; set; }
#endif

        [XmlElement("dhEvento", Order = 5)]
        public string DhEventoField
        {
            get => DhEvento.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhEvento = DateTime.Parse(value);
#else
            set => DhEvento = DateTimeOffset.Parse(value);
#endif

        }

        [XmlAttribute(DataType = "ID")]
        public string Id
        {
            get => "ID" + ((int)TpEvento).ToString() + ChNFe + NSeqEvento.ToString("00");
            set => _ = value;
        }

        [XmlElement("nSeqEvento", Order = 7)]
        public int NSeqEvento { get; set; }

        [XmlElement("tpAmb", Order = 1)]
        public TipoAmbiente TpAmb { get; set; }

        [XmlElement("tpEvento", Order = 6)]
        public TipoEventoNFe TpEvento { get; set; }

        [XmlElement("verEvento", Order = 8)]
        public string VerEvento { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public InfEvento()
        {
        }

        public InfEvento(EventoDetalhe detEvento) => DetEvento = detEvento ?? throw new ArgumentNullException(nameof(detEvento));

        #endregion Public Constructors

        #region Public Methods

        public bool ShouldSerializeCNPJ() => !string.IsNullOrWhiteSpace(CNPJ);

        public bool ShouldSerializeCPF() => !string.IsNullOrWhiteSpace(CPF);

        #endregion Public Methods
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.EventoDetalhe")]
    [ComVisible(true)]
#endif
    [Serializable()]
    [XmlInclude(typeof(DetEventoCanc))]
    [XmlInclude(typeof(DetEventoCCE))]
    [XmlInclude(typeof(DetEventoCancSubst))]
    public class EventoDetalhe : IXmlSerializable
    {
        #region Internal Properties

        internal XmlReader XmlReader { get; set; }

        private static readonly List<string> hasField = new List<string>
        {
            "COrgaoAutor",
        };

        private static readonly BindingFlags bindingFlags = BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.IgnoreCase;

        #endregion Internal Properties

        #region Internal Methods

        internal virtual void ProcessReader()
        {
            if (XmlReader == null)
            {
                return;
            }

            var type = GetType();

            if (XmlReader.HasAttributes)
            {
                if (XmlReader.GetAttribute("versao") != "")
                {
                    var pi = type.GetProperty("versao", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    pi?.SetValue(this, XmlReader.GetAttribute("versao"));
                }
            }

            while (XmlReader.Read())
            {
                if (XmlReader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                SetValue(type);
            }
        }

        internal virtual void SetValue(Type type)
        {
            var pi = GetPropertyInfo(type);

            if (pi == null)
            {
                return;
            }

            SetValue(pi);
        }

        internal virtual void SetValue(PropertyInfo pi) =>
            pi?.SetValue(this, Converter.ToAny(XmlReader.GetValue<object>(XmlReader.Name), pi.PropertyType));

        protected internal PropertyInfo GetPropertyInfo(Type type)
        {
            var pi = hasField.Exists(w => w.ToLower() == XmlReader.Name.ToLower()) ?
                                type.GetProperty(XmlReader.Name + "Field", bindingFlags) :
                                type.GetProperty(XmlReader.Name, bindingFlags);
            return pi;
        }

        #endregion Internal Methods

        #region Public Properties

        [XmlElement("descEvento", Order = 0)]
        public virtual string DescEvento { get; set; }

        [XmlAttribute(AttributeName = "versao", DataType = "token")]
        public virtual string Versao { get; set; }

        #endregion Public Properties

        #region Public Methods

        public XmlSchema GetSchema() => default;

        public void ReadXml(XmlReader reader) => XmlReader = reader;

        public virtual void WriteXml(XmlWriter writer) => writer.WriteAttributeString("versao", Versao);

        #endregion Public Methods
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoCanc")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoCanc : EventoDetalhe
    {
        #region Public Properties

        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "Cancelamento";

        [XmlElement("nProt", Order = 1)]
        public string NProt { get; set; }

        [XmlElement("xJust", Order = 2)]
        public string XJust { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteRaw($@"
            <descEvento>{DescEvento}</descEvento>
            <nProt>{NProt}</nProt>
            <xJust>{XJust}</xJust>");
        }

        #endregion Public Methods
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoCancSubst")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoCancSubst : EventoDetalhe
    {
        #region Public Properties

        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "Cancelamento por substituicao";

        [XmlIgnore]
        public UFBrasil COrgaoAutor { get; set; }

        [XmlElement("cOrgaoAutor", Order = 1)]
        public string COrgaoAutorField
        {
            get => ((int)COrgaoAutor).ToString();
            set => COrgaoAutor = Converter.ToAny<UFBrasil>(value);
        }

        [XmlElement("tpAutor", Order = 2)]
        public TipoAutor TpAutor { get; set; }

        [XmlElement("verAplic", Order = 3)]
        public string VerAplic { get; set; }

        [XmlElement("nProt", Order = 4)]
        public string NProt { get; set; }

        [XmlElement("xJust", Order = 5)]
        public string XJust { get; set; }

        [XmlElement("chNFeRef", Order = 6)]
        public string ChNFeRef { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteRaw($@"<descEvento>{DescEvento}</descEvento>" +
                $@"<cOrgaoAutor>{(int)COrgaoAutor}</cOrgaoAutor>" +
                $@"<tpAutor>{(int)TpAutor}</tpAutor>" +
                $@"<verAplic>{VerAplic}</verAplic>" +
                $@"<nProt>{NProt}</nProt>" +
                $@"<xJust>{XJust}</xJust>" +
                $@"<chNFeRef>{ChNFeRef}</chNFeRef>");
        }

        #endregion Public Methods
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoCCE")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoCCE : EventoDetalhe
    {
        #region Public Properties

        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "Carta de Correcao";

        [XmlElement("xCondUso", Order = 2)]
        public string XCondUso { get; set; } = "A Carta de Correcao e disciplinada pelo paragrafo 1o-A do art. 7o do Convenio S/N, de 15 de dezembro de 1970 e pode ser utilizada para regularizacao de erro ocorrido na emissao de documento fiscal, desde que o erro nao esteja relacionado com: I - as variaveis que determinam o valor do imposto tais como: base de calculo, aliquota, diferenca de preco, quantidade, valor da operacao ou da prestacao; II - a correcao de dados cadastrais que implique mudanca do remetente ou do destinatario; III - a data de emissao ou de saida.";

        [XmlElement("xCorrecao", Order = 1)]
        public string XCorrecao { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            var xcorrecao = XCorrecao;

            xcorrecao = xcorrecao.Replace("&", "&amp;");
            xcorrecao = xcorrecao.Replace("<", "&lt;");
            xcorrecao = xcorrecao.Replace(">", "&gt;");
            xcorrecao = xcorrecao.Replace("\"", "&quot;");
            xcorrecao = xcorrecao.Replace("\\", "&quot;");
            xcorrecao = xcorrecao.Replace("'", "&#39;");

            writer.WriteRaw($@"<descEvento>{DescEvento}</descEvento><xCorrecao>{xcorrecao}</xCorrecao><xCondUso>{XCondUso}</xCondUso>");
        }

        #endregion Public Methods
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoManif")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoManif : EventoDetalhe
    {
        #region Public Properties

        private string DescEventoField;

        /// <summary>
        /// Informe, inclusive obdecendo letras maiúscuas e minúsculas um dos seguintes textos:
        /// </summary>
        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento
        {
            get => DescEventoField;
            set
            {
                DescEventoField = value;
            }
        }

        [XmlElement("xJust", Order = 1)]
        public string XJust { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            if (string.IsNullOrWhiteSpace(XJust) ||
                DescEvento.Equals("Ciencia da Operacao") ||
                DescEvento.Equals("Confirmacao da Operacao"))
            {
                writer.WriteRaw($@"<descEvento>{DescEvento}</descEvento>");
            }
            else
            {
                writer.WriteRaw($@"<descEvento>{DescEvento}</descEvento><xJust>{XJust}</xJust>");
            }
        }

        #endregion Public Methods
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoEPEC")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoEPEC : EventoDetalhe
    {
        internal override void SetValue(PropertyInfo pi)
        {
            if (pi.Name == nameof(Dest))
            {
                XmlReader.Read();
                Dest = new DetEventoEPECDest();
                Dest.UF = XmlReader.GetValue<UFBrasil>(nameof(Dest.UF));
                Dest.CNPJ = XmlReader.GetValue<string>(nameof(Dest.CNPJ));
                Dest.CPF = XmlReader.GetValue<string>(nameof(Dest.CPF));
                Dest.IdEstrangeiro = XmlReader.GetValue<string>(nameof(Dest.IdEstrangeiro));
                Dest.IE = XmlReader.GetValue<string>(nameof(Dest.IE));
                Dest.VNF = UConvert.ToDouble(XmlReader.GetValue<string>(nameof(Dest.VNF)), true);
                Dest.VICMS = UConvert.ToDouble(XmlReader.GetValue<string>(nameof(Dest.VICMS)), true);
                Dest.VST = UConvert.ToDouble(XmlReader.GetValue<string>(nameof(Dest.VST)), true);
                return;
            }

            base.SetValue(pi);
        }

        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "EPEC";

        [XmlIgnore]
        public UFBrasil COrgaoAutor { get; set; }

        [XmlElement("cOrgaoAutor", Order = 1)]
        public int COrgaoAutorField
        {
            get => (int)COrgaoAutor;
            set => COrgaoAutor = (UFBrasil)Enum.Parse(typeof(UFBrasil), value.ToString());
        }

        [XmlElement("tpAutor", Order = 2)]
        public TipoAutor TpAutor { get; set; }

        [XmlElement("verAplic", Order = 3)]
        public string VerAplic { get; set; }

        [XmlIgnore]
#if INTEROP
        public DateTime DhEmi { get; set; }
#else
        public DateTimeOffset DhEmi { get; set; }
#endif

        [XmlElement("dhEmi", Order = 4)]
        public string DhEmiField
        {
            get => DhEmi.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhEmi = DateTime.Parse(value);
#else
            set => DhEmi = DateTimeOffset.Parse(value);
#endif
        }

        [XmlElement("tpNF", Order = 5)]
        public TipoOperacao TpNF { get; set; }

        [XmlElement("IE", Order = 6)]
        public string IE { get; set; }

        [XmlElement("dest", Order = 7)]
        public DetEventoEPECDest Dest { get; set; }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            var linha = $@"<descEvento>{DescEvento}</descEvento>
                       <cOrgaoAutor>{COrgaoAutorField}</cOrgaoAutor>
                       <tpAutor>{(int)TpAutor}</tpAutor>
                       <verAplic>{VerAplic}</verAplic>
                       <dhEmi>{DhEmiField}</dhEmi>
                       <tpNF>{(int)TpNF}</tpNF>
                       <IE>{IE}</IE>";

            linha += $@"<dest>";

            linha += $@"<UF>{Dest.UF}</UF>";
            if (!string.IsNullOrWhiteSpace(Dest.CNPJ))
            {
                linha += $@"<CNPJ>{Dest.CNPJ}</CNPJ>";
            }
            if (!string.IsNullOrWhiteSpace(Dest.CPF))
            {
                linha += $@"<CPF>{Dest.CPF}</CPF>";
            }
            if (!string.IsNullOrWhiteSpace(Dest.IdEstrangeiro))
            {
                linha += $@"<idEstrangeiro>{Dest.IdEstrangeiro}</idEstrangeiro>";
            }
            if (!string.IsNullOrWhiteSpace(Dest.IE))
            {
                linha += $@"<IE>{Dest.IE}</IE>";
            }
            linha += $@"<vNF>{Dest.VNFField}</vNF>";
            linha += $@"<vICMS>{Dest.VICMSField}</vICMS>";
            linha += $@"<vST>{Dest.VSTField}</vST>";

            linha += $@"</dest>";

            writer.WriteRaw(linha);
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoEPECDest")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "dest")]
    public class DetEventoEPECDest
    {
        [XmlElement("UF", Order = 0)]
        public UFBrasil UF { get; set; }

        [XmlElement("CNPJ", Order = 1)]
        public string CNPJ { get; set; }

        [XmlElement("CPF", Order = 1)]
        public string CPF { get; set; }

        [XmlElement("idEstrangeiro", Order = 2)]
        public string IdEstrangeiro { get; set; }

        [XmlElement("IE", Order = 3)]
        public string IE { get; set; }

        [XmlIgnore]
        public double VNF { get; set; }

        [XmlElement("vNF", Order = 4)]
        public string VNFField
        {
            get => VNF.ToString("F2", CultureInfo.InvariantCulture);
            set => VNF = Converter.ToDouble(value);
        }

        [XmlIgnore]
        public double VICMS { get; set; }

        [XmlElement("vICMS", Order = 5)]
        public string VICMSField
        {
            get => VICMS.ToString("F2", CultureInfo.InvariantCulture);
            set => VICMS = Converter.ToDouble(value);
        }

        [XmlIgnore]
        public double VST { get; set; }

        [XmlElement("vST", Order = 6)]
        public string VSTField
        {
            get => VST.ToString("F2", CultureInfo.InvariantCulture);
            set => VST = Converter.ToDouble(value);
        }

        #region ShouldSerialize

        public bool ShouldSerializeCNPJ() => !string.IsNullOrWhiteSpace(CNPJ);

        public bool ShouldSerializeCPF() => !string.IsNullOrWhiteSpace(CPF);

        public bool ShouldSerializeIdEstrangeiro() => !string.IsNullOrWhiteSpace(IdEstrangeiro);

        public bool ShouldSerializeIE() => !string.IsNullOrWhiteSpace(IE);

        #endregion ShouldSerialize
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoCompEntregaNFe")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoCompEntregaNFe : EventoDetalhe
    {
        #region Public Properties

        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "Comprovante de Entrega da NF-e";

        [XmlIgnore]
        public UFBrasil COrgaoAutor { get; set; }

        [XmlElement("cOrgaoAutor", Order = 1)]
        public int COrgaoAutorField
        {
            get => (int)COrgaoAutor;
            set => COrgaoAutor = (UFBrasil)Enum.Parse(typeof(UFBrasil), value.ToString());
        }

        [XmlIgnore]
        public TipoAutor TpAutor { get; set; }

        [XmlElement("tpAutor", Order = 2)]
        public int TpAutorField
        {
            get => (int)TpAutor;
            set
            {
                if (value != (int)TipoAutor.EmpresaEmitente)
                {
                    throw new Exception("Conteúdo da TAG <tpAutor> inválido. Valor aceito 1-Empresa emitente.");
                }

                TpAutor = (TipoAutor)Enum.Parse(typeof(TipoAutor), value.ToString());
            }
        }

        [XmlElement("verAplic", Order = 3)]
        public string VerAplic { get; set; }

        [XmlIgnore]
#if INTEROP
        public DateTime DhEntrega { get; set; }
#else
        public DateTimeOffset DhEntrega { get; set; }
#endif

        [XmlElement("dhEntrega", Order = 4)]
        public string DhEntregaField
        {
            get => DhEntrega.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhEntrega = DateTime.Parse(value);
#else
            set => DhEntrega = DateTimeOffset.Parse(value);
#endif
        }

        [XmlElement("nDoc", Order = 5)]
        public string NDoc { get; set; }

        [XmlElement("xNome", Order = 6)]
        public string XNome { get; set; }

        [XmlElement("latGPS", Order = 7)]
        public string LatGPS { get; set; }

        [XmlElement("longGPS", Order = 8)]
        public string LongGPS { get; set; }

        private string HashComprovanteField;

        [XmlElement("hashComprovante", Order = 9)]
        public string HashComprovante
        {
            get => HashComprovanteField;
            set
            {
                if (Converter.IsSHA1Base64(value))
                {
                    HashComprovanteField = value;
                }
                else
                {
                    HashComprovanteField = Converter.CalculateSHA1Hash(value);
                }
            }
        }

        [XmlIgnore]
#if INTEROP
        public DateTime DhHashComprovante { get; set; }
#else
        public DateTimeOffset DhHashComprovante { get; set; }
#endif

        [XmlElement("dhHashComprovante", Order = 10)]
        public string DhHashComprovanteField
        {
            get => DhHashComprovante.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhHashComprovante = DateTime.Parse(value);
#else
            set => DhHashComprovante = DateTimeOffset.Parse(value);
#endif
        }

        #endregion Public Properties

        #region Public Methods

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            var xml = $@"<descEvento>{DescEvento}</descEvento>
                         <cOrgaoAutor>{COrgaoAutorField}</cOrgaoAutor>
                         <tpAutor>{TpAutorField}</tpAutor>
                         <verAplic>{VerAplic}</verAplic>
                         <dhEntrega>{DhEntregaField}</dhEntrega>
                         <nDoc>{NDoc}</nDoc>
                         <xNome>{XNome}</xNome>";

            if (!string.IsNullOrEmpty(LatGPS) && !string.IsNullOrEmpty(LongGPS))
            {
                xml += $@"<latGPS>{LatGPS}</latGPS>
                          <longGPS>{LongGPS}</longGPS>";
            }

            xml += $@"<hashComprovante>{HashComprovante}</hashComprovante>
                      <dhHashComprovante>{DhHashComprovanteField}</dhHashComprovante>";

            writer.WriteRaw(xml);
        }

        #endregion Public Methods
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoCancCompEntregaNFe")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoCancCompEntregaNFe : EventoDetalhe
    {
        #region Public Properties

        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "Cancelamento Comprovante de Entrega da NF-e";

        [XmlIgnore]
        public UFBrasil COrgaoAutor { get; set; }

        [XmlElement("cOrgaoAutor", Order = 1)]
        public int COrgaoAutorField
        {
            get => (int)COrgaoAutor;
            set => COrgaoAutor = (UFBrasil)Enum.Parse(typeof(UFBrasil), value.ToString());
        }

        [XmlIgnore]
        public TipoAutor TpAutor { get; set; }

        [XmlElement("tpAutor", Order = 2)]
        public int TpAutorField
        {
            get => (int)TpAutor;
            set
            {
                if (value != (int)TipoAutor.EmpresaEmitente)
                {
                    throw new Exception("Conteúdo da TAG <tpAutor> inválido. Valor aceito 1-Empresa emitente.");
                }

                TpAutor = (TipoAutor)Enum.Parse(typeof(TipoAutor), value.ToString());
            }
        }

        [XmlElement("verAplic", Order = 3)]
        public string VerAplic { get; set; }

        [XmlElement("nProtEvento", Order = 4)]
        public string NProtEvento { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteRaw($@"
            <descEvento>{DescEvento}</descEvento>
            <cOrgaoAutor>{COrgaoAutorField}</cOrgaoAutor>
            <tpAutor>{TpAutorField}</tpAutor>
            <verAplic>{VerAplic}</verAplic>
            <nProtEvento>{NProtEvento}</nProtEvento>");
        }

        #endregion Public Methods
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoPedidoProrrogPrazoICMS")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoPedidoProrrogPrazoICMS : EventoDetalhe
    {
        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "Pedido de Prorrogacao";

        [XmlElement("nProt", Order = 1)]
        public string NProt { get; set; }

        [XmlElement("itemPedido", Order = 2)]
        public List<ItemPedidoProrrogPrazoICMS> ItemPedido { get; set; }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            var linha = $@"
            <descEvento>{DescEvento}</descEvento>
            <nProt>{NProt}</nProt>";

            foreach (var item in ItemPedido)
            {
                linha += $@"<itemPedido numItem=" + "\"" + item.NumItem + "\">";
                linha += $@"<qtdeItem>{Convert.ToString(item.QtdeItem, CultureInfo.InvariantCulture)}</qtdeItem>";
                linha += $@"</itemPedido>";
            }

            writer.WriteRaw(linha);
        }


#if INTEROP

        /// <summary>
        /// Adicionar novo elemento a lista
        /// </summary>
        /// <param name="itemPedido">Elemento</param>
        public void AddItemPedido(ItemPedidoProrrogPrazoICMS itemPedido)
        {
            if (ItemPedido == null)
            {
                ItemPedido = new List<ItemPedidoProrrogPrazoICMS>();
            }

            ItemPedido.Add(itemPedido);
        }

        /// <summary>
        /// Retorna o elemento da lista ItemPedido (Utilizado para linguagens diferentes do CSharp que não conseguem pegar o conteúdo da lista)
        /// </summary>
        /// <param name="index">Índice da lista a ser retornado (Começa com 0 (zero))</param>
        /// <returns>Conteúdo do index passado por parâmetro da ItemPedido</returns>
        public ItemPedidoProrrogPrazoICMS GetItemPedido(int index)
        {
            if ((ItemPedido?.Count ?? 0) == 0)
            {
                return default;
            };

            return ItemPedido[index];
        }

        /// <summary>
        /// Retorna a quantidade de elementos existentes na lista ItemPedido
        /// </summary>
        public int GetItemPedidoCount => (ItemPedido != null ? ItemPedido.Count : 0);

#endif
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.ItemPedidoProrrogPrazoICMS")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class ItemPedidoProrrogPrazoICMS
    {
        [XmlAttribute(AttributeName = "numItem", DataType = "token")]
        public int NumItem { get; set; }

        [XmlElement("qtdeItem", Order = 0)]
        public double QtdeItem { get; set; }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoCancPedidoProrrogPrazoICMS")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoCancPedidoProrrogPrazoICMS : EventoDetalhe
    {
        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "Cancelamento de Pedido de Prorrogacao";

        [XmlElement("idPedidoCancelado", Order = 1)]
        public string IdPedidoCancelado { get; set; }

        [XmlElement("nProt", Order = 2)]
        public string NProt { get; set; }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteRaw($@"
            <descEvento>{DescEvento}</descEvento>
            <idPedidoCancelado>{IdPedidoCancelado}</idPedidoCancelado>
            <nProt>{NProt}</nProt>");
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoSEFAZ")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoSEFAZ : EventoDetalhe
    {
        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoCTeAutorizado")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoCTeAutorizado : EventoDetalhe
    {
        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "CT-e Autorizado";

        private DetEventoCTeAutorizadoCTe CTeField;
        private DetEventoCTeAutorizadoEmit EmitField;

        [XmlElement("CTe", Order = 1)]
        public DetEventoCTeAutorizadoCTe CTe
        {
            get => CTeField;
            set
            {
                if (CTeField == null)
                {
                    CTeField = new DetEventoCTeAutorizadoCTe();
                }

                CTeField = value;
            }
        }

        [XmlElement("emit", Order = 2)]
        public DetEventoCTeAutorizadoEmit Emit
        {
            get => EmitField;
            set
            {
                if (EmitField == null)
                {
                    EmitField = new DetEventoCTeAutorizadoEmit();
                }

                EmitField = value;
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteRaw($@"
                <descEvento>{DescEvento}</descEvento>
                <CTe>
                    <chCTe>{CTe.ChCTe}</chCTe>
                    <modal>{(int)CTe.Modal:00}</modal>
                    <dhEmi>{CTe.DhEmiField}</dhEmi>
                    <nProt>{CTe.NProt}</nProt>
                    <dhRecbto>{CTe.DhRecbtoField}</dhRecbto>
                </CTe>
                <emit>
                    <CNPJ>{Emit.CNPJ}</CNPJ>
                    <IE>{Emit.IE}</IE>
                    <xNome>{Emit.XNome}</xNome>
                </emit>");
        }

        internal override void ProcessReader()
        {
            if (XmlReader == null)
            {
                return;
            }

            var xml = new XmlDocument();
            xml.Load(XmlReader);

            if (xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao") != null)
            {
                Versao = xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao").Value;
            }

            CTe = XMLUtility.Deserializar<DetEventoCTeAutorizadoCTe>(xml.GetElementsByTagName("CTe")[0].OuterXml);
            Emit = XMLUtility.Deserializar<DetEventoCTeAutorizadoEmit>(xml.GetElementsByTagName("emit")[0].OuterXml);
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoCTeAutorizadoCTe")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot("CTe", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoCTeAutorizadoCTe : XMLBase
    {
        [XmlElement("chCTe")]
        public string ChCTe { get; set; }

        [XmlElement("modal")]
        public ModalidadeTransporteCTe Modal { get; set; }

        [XmlIgnore]
#if INTEROP
        public DateTime DhEmi { get; set; }
#else
        public DateTimeOffset DhEmi { get; set; }
#endif

        [XmlElement("dhEmi")]
        public string DhEmiField
        {
            get => DhEmi.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhEmi = DateTime.Parse(value);
#else
            set => DhEmi = DateTimeOffset.Parse(value);
#endif
        }

        [XmlElement("nProt")]
        public string NProt { get; set; }

        [XmlIgnore]
#if INTEROP
        public DateTime DhRecbto { get; set; }
#else
        public DateTimeOffset DhRecbto { get; set; }
#endif

        [XmlElement("dhRecbto")]
        public string DhRecbtoField
        {
            get => DhRecbto.ToString("yyyy-MM-ddTHH:mm:ss");
#if INTEROP
            set => DhRecbto = DateTime.Parse(value);
#else
            set => DhRecbto = DateTimeOffset.Parse(value);
#endif
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoCTeAutorizadoEmit")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot("emit", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoCTeAutorizadoEmit
    {
        [XmlElement("CNPJ")]
        public string CNPJ { get; set; }

        [XmlElement("IE")]
        public string IE { get; set; }

        [XmlElement("xNome")]
        public string XNome { get; set; }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoMDFeAutorizadoComCTe")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoMDFeAutorizadoComCTe : EventoDetalhe
    {
        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "MDF-e Autorizado com CT-e";

        [XmlElement("cOrgaoAutor", Order = 1)]
        public string COrgaoAutor { get; set; }

        [XmlElement("tpAutor", Order = 2)]
        public string TpAutor { get; set; }

        [XmlElement("verAplic", Order = 3)]
        public string VerAplic { get; set; }

        private DetEventoMDFeAutorizadoComCTeMDFe MDFeField;
        private DetEventoMDFeAutorizadoComCTeEmit EmitField;

        [XmlElement("MDFe", Order = 4)]
        public DetEventoMDFeAutorizadoComCTeMDFe MDFe
        {
            get => MDFeField;
            set
            {
                if (MDFeField == null)
                {
                    MDFeField = new DetEventoMDFeAutorizadoComCTeMDFe();
                }

                MDFeField = value;
            }
        }

        [XmlElement("emit", Order = 5)]
        public DetEventoMDFeAutorizadoComCTeEmit Emit
        {
            get => EmitField;
            set
            {
                if (EmitField == null)
                {
                    EmitField = new DetEventoMDFeAutorizadoComCTeEmit();
                }

                EmitField = value;
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteRaw($@"
                <descEvento>{DescEvento}</descEvento>
                <cOrgaoAutor>{COrgaoAutor}</cOrgaoAutor> 
                <tpAutor>{TpAutor}</tpAutor> 
                <verAplic>{VerAplic}</verAplic> 
                <MDFe>
                    <chMDFe>{MDFe.ChMDFe}</chMDFe>  
                    <chCTe>{MDFe.ChCTe}</chCTe>
                    <modal>{(int)MDFe.Modal:00}</modal>
                    <dhEmi>{MDFe.DhEmiField}</dhEmi>
                    <nProt>{MDFe.NProt}</nProt>
                    <dhRecbto>{MDFe.DhRecbtoField}</dhRecbto>
                </MDFe>
                <emit>
                    <CNPJ>{Emit.CNPJ}</CNPJ>
                    <IE>{Emit.IE}</IE>
                    <xNome>{Emit.XNome}</xNome>
                </emit>");
        }

        internal override void ProcessReader()
        {
            if (XmlReader == null)
            {
                return;
            }

            var xml = new XmlDocument();
            xml.Load(XmlReader);

            if (xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao") != null)
            {
                Versao = xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao").Value;
            }
            if (xml.GetElementsByTagName("cOrgaoAutor") != null)
            {
                COrgaoAutor = xml.GetElementsByTagName("cOrgaoAutor")[0].InnerText;
            }
            if (xml.GetElementsByTagName("tpAutor") != null)
            {
                TpAutor = xml.GetElementsByTagName("tpAutor")[0].InnerText;
            }
            if (xml.GetElementsByTagName("verAplic") != null)
            {
                VerAplic = xml.GetElementsByTagName("verAplic")[0].InnerText;
            }

            MDFe = XMLUtility.Deserializar<DetEventoMDFeAutorizadoComCTeMDFe>(xml.GetElementsByTagName("MDFe")[0].OuterXml);
            Emit = XMLUtility.Deserializar<DetEventoMDFeAutorizadoComCTeEmit>(xml.GetElementsByTagName("emit")[0].OuterXml);
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoMDFeAutorizadoComCTeMDFe")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot("MDFe", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoMDFeAutorizadoComCTeMDFe : XMLBase
    {
        [XmlElement("chMDFe")]
        public string ChMDFe { get; set; }

        [XmlElement("chCTe")]
        public string ChCTe { get; set; }

        [XmlElement("modal")]
        public ModalidadeTransporteCTe Modal { get; set; }

        [XmlIgnore]
#if INTEROP
        public DateTime DhEmi { get; set; }
#else
        public DateTimeOffset DhEmi { get; set; }
#endif

        [XmlElement("dhEmi")]
        public string DhEmiField
        {
            get => DhEmi.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhEmi = DateTime.Parse(value);
#else
            set => DhEmi = DateTimeOffset.Parse(value);
#endif
        }

        [XmlElement("nProt")]
        public string NProt { get; set; }

        [XmlIgnore]
#if INTEROP
        public DateTime DhRecbto { get; set; }
#else
        public DateTimeOffset DhRecbto { get; set; }
#endif

        [XmlElement("dhRecbto")]
        public string DhRecbtoField
        {
            get => DhRecbto.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhRecbto = DateTime.Parse(value);
#else
            set => DhRecbto = DateTimeOffset.Parse(value);
#endif
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoMDFeAutorizadoComCTeEmit")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot("emit", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoMDFeAutorizadoComCTeEmit : DetEventoCTeAutorizadoEmit { }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoComprovanteEntregaCTe")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoComprovanteEntregaCTe : EventoDetalhe
    {
        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "Comprovante de Entrega do CT-e";

        [XmlElement("cOrgaoAutor", Order = 1)]
        public string COrgaoAutor { get; set; }

        [XmlElement("tpAutor", Order = 2)]
        public string TpAutor { get; set; }

        [XmlElement("verAplic", Order = 3)]
        public string VerAplic { get; set; }

        private DetEventoComprovanteEntregaCTeCTe CTeField;

        [XmlElement("CTe", Order = 4)]
        public DetEventoComprovanteEntregaCTeCTe CTe
        {
            get => CTeField;
            set
            {
                if (CTeField == null)
                {
                    CTeField = new DetEventoComprovanteEntregaCTeCTe();
                }

                CTeField = value;
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteRaw($@"
                <descEvento>{DescEvento}</descEvento>
                <cOrgaoAutor>{COrgaoAutor}</cOrgaoAutor> 
                <tpAutor>{TpAutor}</tpAutor> 
                <verAplic>{VerAplic}</verAplic> 
                <CTe>
                    <chCTe>{CTe.ChCTe}</chCTe>  
                    <nProtCTe>{CTe.NProtCTe}</nProtCTe>
                    <dhEntrega>{CTe.DhEntregaField}</dhEntrega>
                    <nDoc>{CTe.NDoc}</nDoc>
                    <xNome>{CTe.XNome}</xNome>
                    <hashEntregaCTe>{CTe.HashEntregaCTe}</hashEntregaCTe>
                    <dhHashEntregaCTe>{CTe.DhHashEntregaCTeField}</dhHashEntregaCTe>
                </CTe>");
        }

        internal override void ProcessReader()
        {
            if (XmlReader == null)
            {
                return;
            }

            var xml = new XmlDocument();
            xml.Load(XmlReader);

            if (xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao") != null)
            {
                Versao = xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao").Value;
            }
            if (xml.GetElementsByTagName("cOrgaoAutor") != null)
            {
                COrgaoAutor = xml.GetElementsByTagName("cOrgaoAutor")[0].InnerText;
            }
            if (xml.GetElementsByTagName("tpAutor") != null)
            {
                TpAutor = xml.GetElementsByTagName("tpAutor")[0].InnerText;
            }
            if (xml.GetElementsByTagName("verAplic") != null)
            {
                VerAplic = xml.GetElementsByTagName("verAplic")[0].InnerText;
            }

            CTe = XMLUtility.Deserializar<DetEventoComprovanteEntregaCTeCTe>(xml.GetElementsByTagName("CTe")[0].OuterXml);
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoComprovanteEntregaCTeCTe")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot("CTe", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoComprovanteEntregaCTeCTe
    {
        [XmlElement("chCTe")]
        public string ChCTe { get; set; }

        [XmlElement("nProtCTe")]
        public string NProtCTe { get; set; }

        [XmlIgnore]
#if INTEROP
        public DateTime DhEntrega { get; set; }
#else
        public DateTimeOffset DhEntrega { get; set; }
#endif

        [XmlElement("dhEntrega")]
        public string DhEntregaField
        {
            get => DhEntrega.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhEntrega = DateTime.Parse(value);
#else
            set => DhEntrega = DateTimeOffset.Parse(value);
#endif
        }

        [XmlElement("nDoc")]
        public string NDoc { get; set; }

        [XmlElement("xNome")]
        public string XNome { get; set; }

        [XmlElement("hashEntregaCTe")]
        public string HashEntregaCTe { get; set; }


        [XmlIgnore]
#if INTEROP
        public DateTime DhHashEntregaCTe { get; set; }
#else
        public DateTimeOffset DhHashEntregaCTe { get; set; }
#endif

        [XmlElement("dhHashEntregaCTe")]
        public string DhHashEntregaCTeField
        {
            get => DhHashEntregaCTe.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhHashEntregaCTe = DateTime.Parse(value);
#else
            set => DhHashEntregaCTe = DateTimeOffset.Parse(value);
#endif
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoCancelamentoComprovanteEntregaCTe")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoCancelamentoComprovanteEntregaCTe : EventoDetalhe
    {
        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "Cancelamento Comprovante de Entrega do CT-e";

        [XmlElement("cOrgaoAutor", Order = 1)]
        public string COrgaoAutor { get; set; }

        [XmlElement("tpAutor", Order = 2)]
        public string TpAutor { get; set; }

        [XmlElement("verAplic", Order = 3)]
        public string VerAplic { get; set; }

        [XmlElement("chCTe", Order = 4)]
        public string ChCTe { get; set; }

        [XmlElement("nProtCTeCanc", Order = 4)]
        public string NProtCTeCanc { get; set; }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteRaw($@"
                <descEvento>{DescEvento}</descEvento>
                <cOrgaoAutor>{COrgaoAutor}</cOrgaoAutor> 
                <tpAutor>{TpAutor}</tpAutor> 
                <verAplic>{VerAplic}</verAplic> 
                <chCTe>{ChCTe}</chCTe> 
                <nProtCTeCanc>{NProtCTeCanc}</nProtCTeCanc>");
        }

        internal override void ProcessReader()
        {
            if (XmlReader == null)
            {
                return;
            }

            var xml = new XmlDocument();
            xml.Load(XmlReader);

            if (xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao") != null)
            {
                Versao = xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao").Value;
            }
            if (xml.GetElementsByTagName("cOrgaoAutor") != null)
            {
                COrgaoAutor = xml.GetElementsByTagName("cOrgaoAutor")[0].InnerText;
            }
            if (xml.GetElementsByTagName("tpAutor") != null)
            {
                TpAutor = xml.GetElementsByTagName("tpAutor")[0].InnerText;
            }
            if (xml.GetElementsByTagName("verAplic") != null)
            {
                VerAplic = xml.GetElementsByTagName("verAplic")[0].InnerText;
            }
            if (xml.GetElementsByTagName("chCTe") != null)
            {
                ChCTe = xml.GetElementsByTagName("chCTe")[0].InnerText;
            }
            if (xml.GetElementsByTagName("nProtCTeCanc") != null)
            {
                NProtCTeCanc = xml.GetElementsByTagName("nProtCTeCanc")[0].InnerText;
            }
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoAverbacaoExportacao")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoAverbacaoExportacao : EventoDetalhe
    {
        [XmlElement("descEvento", Order = 0)]
        public override string DescEvento { get; set; } = "Averbação para Exportação";

        [XmlElement("tpAutor", Order = 2)]
        public string TpAutor { get; set; }

        [XmlElement("verAplic", Order = 3)]
        public string VerAplic { get; set; }

        [XmlElement("itensAverbados", Order = 4)]
        public List<DetEventoAverbacaoExportacaoItensAverbados> ItensAverbados { get; set; } = new List<DetEventoAverbacaoExportacaoItensAverbados>();

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            var writeRaw = $@"
                <descEvento>{DescEvento}</descEvento>
                <tpAutor>{TpAutor}</tpAutor> 
                <verAplic>{VerAplic}</verAplic>";

            foreach (var item in ItensAverbados)
            {
                writeRaw += $@"<itensAverbados>
                    <dhEmbarque>{item.DhEmbarqueField}</dhEmbarque>
                    <dhAverbacao>{item.DhAverbacaoField}</dhAverbacao>
					<nDue>{item.NDue}</nDue>
					<nItem>{item.NItem}</nItem>
					<nItemDue>{item.NItemDue}</nItemDue>
					<qItem>{item.QItem}</qItem>
                    <motAlteracao>{item.MotAlteracao}</motAlteracao>
                    </itensAverbados>";
            }

            writer.WriteRaw(writeRaw);
        }

        internal override void ProcessReader()
        {
            if (XmlReader == null)
            {
                return;
            }

            var xml = new XmlDocument();
            xml.Load(XmlReader);

            if (xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao") != null)
            {
                Versao = xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao").Value;
            }
            if (xml.GetElementsByTagName("tpAutor") != null)
            {
                TpAutor = xml.GetElementsByTagName("tpAutor")[0].InnerText;
            }
            if (xml.GetElementsByTagName("verAplic") != null)
            {
                VerAplic = xml.GetElementsByTagName("verAplic")[0].InnerText;
            }

            var detEventoNodeList = xml.GetElementsByTagName("detEvento");
            foreach (var item in detEventoNodeList)
            {
                var detEventoElement = (XmlElement)item;

                var itensAverbadosNodeList = detEventoElement.GetElementsByTagName("itensAverbados");

                foreach (var itemAverbado in itensAverbadosNodeList)
                {
                    var itensAverbadosElement = (XmlElement)itemAverbado;

                    ItensAverbados.Add(new DetEventoAverbacaoExportacaoItensAverbados
                    {
                        DhEmbarqueField = (itensAverbadosElement.GetElementsByTagName("dhEmbarque").Count > 0 ? itensAverbadosElement.GetElementsByTagName("dhEmbarque")[0].InnerText : ""),
                        DhAverbacaoField = (itensAverbadosElement.GetElementsByTagName("dhAverbacao").Count > 0 ? itensAverbadosElement.GetElementsByTagName("dhAverbacao")[0].InnerText : ""),
                        NDue = (itensAverbadosElement.GetElementsByTagName("nDue").Count > 0 ? itensAverbadosElement.GetElementsByTagName("nDue")[0].InnerText : ""),
                        NItem = (itensAverbadosElement.GetElementsByTagName("nItem").Count > 0 ? itensAverbadosElement.GetElementsByTagName("nItem")[0].InnerText : ""),
                        NItemDue = (itensAverbadosElement.GetElementsByTagName("nItemDue").Count > 0 ? itensAverbadosElement.GetElementsByTagName("nItemDue")[0].InnerText : ""),
                        QItem = (itensAverbadosElement.GetElementsByTagName("qItem").Count > 0 ? itensAverbadosElement.GetElementsByTagName("qItem")[0].InnerText : ""),
                        MotAlteracao = (itensAverbadosElement.GetElementsByTagName("motAlteracao").Count > 0 ? itensAverbadosElement.GetElementsByTagName("motAlteracao")[0].InnerText : ""),
                    });
                }
            }
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoAverbacaoExportacaoItensAverbados")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot("itensAverbados", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoAverbacaoExportacaoItensAverbados
    {
        [XmlIgnore]
#if INTEROP
        public DateTime DhEmbarque { get; set; }
#else
        public DateTimeOffset DhEmbarque { get; set; }
#endif

        [XmlElement("dhEmbarque")]
        public string DhEmbarqueField
        {
            get => DhEmbarque.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhEmbarque = DateTime.Parse(value);
#else
            set => DhEmbarque = DateTimeOffset.Parse(value);
#endif
        }

        [XmlIgnore]
#if INTEROP
        public DateTime DhAverbacao { get; set; }
#else
        public DateTimeOffset DhAverbacao { get; set; }
#endif

        [XmlElement("dhAverbacao")]
        public string DhAverbacaoField
        {
            get => DhAverbacao.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhAverbacao = DateTime.Parse(value);
#else
            set => DhAverbacao = DateTimeOffset.Parse(value);
#endif
        }

        [XmlElement("nDue")]
        public string NDue { get; set; }

        [XmlElement("nItem")]
        public string NItem { get; set; }

        [XmlElement("nItemDue")]
        public string NItemDue { get; set; }

        [XmlElement("qItem")]
        public string QItem { get; set; }

        [XmlElement("motAlteracao")]
        public string MotAlteracao { get; set; }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoVistoriaSuframaSEFAZ")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoVistoriaSuframaSEFAZ : EventoDetalhe
    {
        [XmlElement("descEvento")]
        public override string DescEvento { get; set; } = "Vistoria SUFRAMA - SEFAZ";

        [XmlElement("PINe")]
        public string PINe { get; set; }

        [XmlIgnore]
#if INTEROP
        public DateTime DVistoria { get; set; }
#else
        public DateTimeOffset DVistoria { get; set; }
#endif

        [XmlElement("dVistoria")]
        public string DVistoriaField
        {
            get => DVistoria.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DVistoria = DateTime.Parse(value);
#else
            set => DVistoria = DateTimeOffset.Parse(value);
#endif
        }

        [XmlElement("locVistoria")]
        public string LocVistoria { get; set; }

        [XmlElement("postoVistoria")]
        public string PostoVistoria { get; set; }

        [XmlElement("xHistorico")]
        public string XHistorico { get; set; }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            var writeRaw = $@"
                <descEvento>{DescEvento}</descEvento>
                <PINe>{PINe}</PINe> 
                <dVistoria>{DVistoriaField}</dVistoria> 
                <locVistoria>{LocVistoria}</locVistoria> 
                <postoVistoria>{PostoVistoria}</postoVistoria> 
                <xHistorico>{XHistorico}</xHistorico>";

            writer.WriteRaw(writeRaw);
        }

        internal override void ProcessReader()
        {
            if (XmlReader == null)
            {
                return;
            }

            var xml = new XmlDocument();
            xml.Load(XmlReader);

            if (xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao") != null)
            {
                Versao = xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao").Value;
            }
            if (xml.GetElementsByTagName("PINe") != null)
            {
                PINe = xml.GetElementsByTagName("PINe")[0].InnerText;
            }
            if (xml.GetElementsByTagName("dVistoria") != null)
            {
                DVistoriaField = xml.GetElementsByTagName("dVistoria")[0].InnerText;
            }
            if (xml.GetElementsByTagName("locVistoria") != null)
            {
                LocVistoria = xml.GetElementsByTagName("locVistoria")[0].InnerText;
            }
            if (xml.GetElementsByTagName("postoVistoria") != null)
            {
                PostoVistoria = xml.GetElementsByTagName("postoVistoria")[0].InnerText;
            }
            if (xml.GetElementsByTagName("xHistorico") != null)
            {
                XHistorico = xml.GetElementsByTagName("xHistorico")[0].InnerText;
            }
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoVistoriaSuframa")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoVistoriaSuframa : DetEventoVistoriaSuframaSEFAZ
    {
        [XmlElement("descEvento")]
        public override string DescEvento { get; set; } = "Vistoria SUFRAMA";
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoInternalizacaoSUFRAMA")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DetEventoInternalizacaoSUFRAMA : DetEventoVistoriaSuframaSEFAZ
    {
        [XmlElement("descEvento")]
        public override string DescEvento { get; set; } = "Confirmacao de Internalizacao da Mercadoria na SUFRAMA";
    }


#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoInsucessoEntregaNFe")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoInsucessoEntregaNFe : EventoDetalhe
    {
        /// <summary>
        /// Descrição do evento
        /// </summary>
        [XmlElement("descEvento")]
        public override string DescEvento { get; set; } = "Insucesso na Entrega da NF-e";

        /// <summary>
        /// Código do Órgão Autor do Evento. Informar o Código da F da Chave de Acesso para este Evento.
        /// </summary>
        [XmlIgnore]
        public UFBrasil COrgaoAutor { get; set; }

        [XmlElement("cOrgaoAutor")]
        public int COrgaoAutorField
        {
            get => (int)COrgaoAutor;
            set => COrgaoAutor = (UFBrasil)Enum.Parse(typeof(UFBrasil), value.ToString());
        }

        /// <summary>
        /// Versão do aplicativo do Autor do Evento. 
        /// </summary>
        [XmlElement("verAplic")]
        public string VerAplic { get; set; }

        /// <summary>
        /// Data e hora da tentativa de entrega
        /// Formato= AAAA-MM-DDTHH:MM:SS TZD
        /// </summary>
        [XmlIgnore]
#if INTEROP
        public DateTime DhTentativaEntrega { get; set; }
#else
        public DateTimeOffset DhTentativaEntrega { get; set; }
#endif

        [XmlElement("dhTentativaEntrega")]
        public string DhTentativaEntregaField
        {
            get => DhTentativaEntrega.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhTentativaEntrega = DateTime.Parse(value);
#else
            set => DhTentativaEntrega = DateTimeOffset.Parse(value);
#endif
        }

        /// <summary>
        /// Número da tentativa de entrega que não teve sucesso
        /// </summary>
        [XmlElement("nTentativa")]
        public int NTentativa { get; set; }

        /// <summary>
        /// Motivo do insucesso da entrega
        /// </summary>
        [XmlElement("tpMotivo")]
        public TipoMotivoInsucessoEntrega TpMotivo { get; set; }

        /// <summary>
        /// Justificativa do motivo do insucesso. Informar apenas para TpMotivo=4-Outros
        /// </summary>
        [XmlElement("xJustMotivo")]
        public string XJustMotivo { get; set; }

        /// <summary>
        /// Latitude do ponto de entrega (Coordenada GPS)
        /// </summary>
        [XmlElement("latGPS")]
        public string LatGPS { get; set; }

        /// <summary>
        /// Longitude do ponto de entrega (Coordenada GPS)+
        /// </summary>
        [XmlElement("longGPS")]
        public string LongGPS { get; set; }

        private string HashTentativaEntregaField;

        /// <summary>
        /// Hash SHA-1, no formato Base64, resultante da concatenação de: Chave de Acesso da NF-e + Base64 da imagem capturada na tentativa da entrega (ex: imagem capturada da assinatura eletrônica, digital do recebedor, foto, etc). 
        /// 
        /// Nota 1: A critério do autor do evento, este campo pode ser utilizado como índice para acesso as informações do Insucesso na Entrega da NF-e.
        /// Nota 2: A SEFAZ não tem nenhum controle sobre a informação deste campo.
        /// 
        /// Propriedade, se não for informado em Base64, automaticamente converte o conteúdo para Base64, facilitando para o desenvolvedor.
        /// </summary>
        [XmlElement("hashTentativaEntrega")]
        public string HashTentativaEntrega
        {
            get => HashTentativaEntregaField;
            set
            {
                if (Converter.IsSHA1Base64(value))
                {
                    HashTentativaEntregaField = value;
                }
                else
                {
                    HashTentativaEntregaField = Converter.CalculateSHA1Hash(value);
                }
            }
        }

        /// <summary>
        /// Data e hora da geração do hash da tentativa de entrega. 
        /// Formato AAAA-MMDDThh:mm:ssTZD.
        /// </summary>
        [XmlIgnore]
#if INTEROP
        public DateTime DhHashTentativaEntrega { get; set; }
#else
        public DateTimeOffset DhHashTentativaEntrega { get; set; }
#endif

        [XmlElement("dhHashTentativaEntrega")]
        public string DhHashTentativaEntregaField
        {
            get => DhHashTentativaEntrega.ToString("yyyy-MM-ddTHH:mm:sszzz");
#if INTEROP
            set => DhHashTentativaEntrega = DateTime.Parse(value);
#else
            set => DhHashTentativaEntrega = DateTimeOffset.Parse(value);
#endif
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            var xml = $@"<descEvento>{DescEvento}</descEvento>
                         <cOrgaoAutor>{COrgaoAutorField}</cOrgaoAutor>
                         <verAplic>{VerAplic}</verAplic>
                         <dhTentativaEntrega>{DhTentativaEntregaField}</dhTentativaEntrega>";

            if (NTentativa > 0)
            {
                xml += $@"<nTentativa>{NTentativa}</nTentativa>";
            }

            xml += $@"<tpMotivo>{(int)TpMotivo}</tpMotivo>";

            if (TpMotivo == TipoMotivoInsucessoEntrega.Outros)
            {
                xml += $@"<xJustMotivo>{XJustMotivo}</xJustMotivo>";
            }

            if (!string.IsNullOrEmpty(LatGPS) && !string.IsNullOrEmpty(LongGPS))
            {
                xml += $@"<latGPS>{LatGPS}</latGPS>
                          <longGPS>{LongGPS}</longGPS>";
            }


            xml += $@"<hashTentativaEntrega>{HashTentativaEntrega}</hashTentativaEntrega>
                         <dhHashTentativaEntrega>{DhHashTentativaEntregaField}</dhHashTentativaEntrega>";

            writer.WriteRaw(xml);
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoCancelamentoInsucessoEntregaNFe")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoCancelamentoInsucessoEntregaNFe : EventoDetalhe
    {
        /// <summary>
        /// Descrição do evento
        /// </summary>
        [XmlElement("descEvento")]
        public override string DescEvento { get; set; } = "Cancelamento Insucesso na Entrega da NF-e";

        /// <summary>
        /// Código do Órgão Autor do Evento. Informar o Código da F da Chave de Acesso para este Evento.
        /// </summary>
        [XmlIgnore]
        public UFBrasil COrgaoAutor { get; set; }

        [XmlElement("cOrgaoAutor")]
        public int COrgaoAutorField
        {
            get => (int)COrgaoAutor;
            set => COrgaoAutor = (UFBrasil)Enum.Parse(typeof(UFBrasil), value.ToString());
        }

        /// <summary>
        /// Versão do aplicativo do Autor do Evento. 
        /// </summary>
        [XmlElement("verAplic")]
        public string VerAplic { get; set; }

        /// <summary>
        /// Informar o número do Protocolo de Autorização do Evento da NF-e a que se refere este cancelamento.
        /// </summary>
        [XmlElement("nProtEvento")]
        public string NProtEvento { get; set; }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            var xml = $@"<descEvento>{DescEvento}</descEvento>
                         <cOrgaoAutor>{COrgaoAutorField}</cOrgaoAutor>
                         <verAplic>{VerAplic}</verAplic>
                         <nProtEvento>{NProtEvento}</nProtEvento>";

            writer.WriteRaw(xml);
        }
    }


#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoAtorInteressadoNFe")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoAtorInteressadoNFe : EventoDetalhe
    {
        /// <summary>
        /// Descrição do evento
        /// </summary>
        [XmlElement("descEvento")]
        public override string DescEvento { get; set; } = "Ator interessado na NF-e";

        /// <summary>
        /// Código da UF do emitente do Evento
        /// </summary>
        [XmlIgnore]
        public UFBrasil COrgaoAutor { get; set; }

        [XmlElement("cOrgaoAutor")]
        public int COrgaoAutorField
        {
            get => (int)COrgaoAutor;
            set => COrgaoAutor = (UFBrasil)Enum.Parse(typeof(UFBrasil), value.ToString());
        }

        /// <summary>
        /// Tipo do autor gerador do evento
        /// </summary>
        [XmlElement("tpAutor")]
        public TipoAutorGeradorEvento TpAutor { get; set; }

        /// <summary>
        /// Versão do aplicativo do Autor do Evento. 
        /// </summary>
        [XmlElement("verAplic")]
        public string VerAplic { get; set; }

        /// <summary>
        /// CNPJs ou CPFs autorizados a fazer download do XML da NFe
        /// </summary>
        [XmlElement("autXML")]
        public AutXML AutXML { get; set; } = new AutXML();

        /// <summary>
        /// Tipo de autorização do evento do ator interessado na NFe
        /// </summary>
        [XmlElement("tpAutorizacao")]
#if INTEROP
        public TipoAutorizacao TpAutorizacao { get; set; } = (TipoAutorizacao)(-1);

#else
        public TipoAutorizacao? TpAutorizacao { get; set; }
#endif

        /// <summary>
        /// Texto Fixo com as Condição de uso do tipo de autorização para o transportador
        /// </summary>
        public string XCondUso { get; set; } = "O emitente ou destinatário da NF-e, declara que permite o transportador declarado no campo CNPJ/CPF deste evento a autorizar os transportadores subcontratados ou redespachados a terem acesso ao download da NF-e";

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            var xml = $@"<descEvento>{DescEvento}</descEvento>
                         <cOrgaoAutor>{COrgaoAutorField}</cOrgaoAutor>
                         <tpAutor>{(int)TpAutor}</tpAutor>
                         <verAplic>{VerAplic}</verAplic>
                         <autXML>";

            if (!string.IsNullOrEmpty(AutXML.CPF))
            {
                xml += $"<CPF>{AutXML.CPF}</CPF>";
            }
            else
            {
                xml += $"<CNPJ>{AutXML.CNPJ}</CNPJ>";
            }

            xml += $@"</autXML>";

#if INTEROP
            if (TpAutorizacao != (TipoAutorizacao)(-1))
#else
            if (TpAutorizacao != null)
#endif
            {
                xml += $@"<tpAutorizacao>{(int)TpAutorizacao}</tpAutorizacao>";
            }

            if (TpAutorizacao == TipoAutorizacao.Permite && !string.IsNullOrWhiteSpace(XCondUso))
            {
                xml += $@"<xCondUso>{XCondUso}</xCondUso>";
            }

            writer.WriteRaw(xml);
        }

        internal override void ProcessReader()
        {
            if (XmlReader == null)
            {
                return;
            }

            var xml = new XmlDocument();
            xml.Load(XmlReader);

            if (xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao") != null)
            {
                Versao = xml.GetElementsByTagName("detEvento")[0].Attributes.GetNamedItem("versao").Value;
            }
            if (xml.GetElementsByTagName("cOrgaoAutor").Count > 0)
            {
                COrgaoAutor = (UFBrasil)Convert.ToInt32(xml.GetElementsByTagName("cOrgaoAutor")[0].InnerText);
            }
            if (xml.GetElementsByTagName("tpAutor").Count > 0)
            {
                TpAutor = (TipoAutorGeradorEvento)Convert.ToInt32(xml.GetElementsByTagName("tpAutor")[0].InnerText);
            }
            if (xml.GetElementsByTagName("verAplic").Count > 0)
            {
                VerAplic = xml.GetElementsByTagName("verAplic")[0].InnerText;
            }
            if (xml.GetElementsByTagName("tpAutorizacao").Count > 0)
            {
                TpAutorizacao = (TipoAutorizacao)Convert.ToInt32(xml.GetElementsByTagName("tpAutorizacao")[0].InnerText);
            }

            var detEventoNodeList = xml.GetElementsByTagName("detEvento");
            foreach (var item in detEventoNodeList)
            {
                var detEventoElement = (XmlElement)item;

                var autXMLNodeList = detEventoElement.GetElementsByTagName("autXML");

                foreach (var itemAutXML in autXMLNodeList)
                {
                    var autXMLElement = (XmlElement)itemAutXML;

                    if (autXMLElement.GetElementsByTagName("CPF").Count > 0)
                    {
                        AutXML.CPF = autXMLElement.GetElementsByTagName("CPF")[0].InnerText;
                    }
                    else
                    {
                        AutXML.CNPJ = autXMLElement.GetElementsByTagName("CNPJ")[0].InnerText;
                    }
                }
            }
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoConciliacaoFinanceira")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoConciliacaoFinanceira : EventoDetalhe
    {
        /// <summary>
        /// Descrição do evento
        /// </summary>
        [XmlElement("descEvento")]
        public override string DescEvento { get; set; } = "ECONF";

        /// <summary>
        /// Versão do aplicativo do Autor do Evento. 
        /// </summary>
        [XmlElement("verAplic")]
        public string VerAplic { get; set; }

        /// <summary>
        /// Grupo de detalhamento do pagamento
        /// </summary>
        [XmlElement("detPag")]
        public List<DetPagECONF> DetPag { get; set; }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            var xml = $@"<descEvento>{DescEvento}</descEvento>
                         <verAplic>{VerAplic}</verAplic>";

            foreach (var detpag in DetPag)
            {
                xml += "<detPag>";

#if INTEROP
                if (detpag.IndPag != (IndicadorPagamento)(-1))
#else
                if (detpag.IndPag != null)
#endif
                {
                    xml += $"<indPag>{(int)detpag.IndPag}</indPag>";
                }

                xml += $"<tPag>{((int)detpag.TPag).ToString("00")}</tPag>";

                if (detpag.TPag == MeioPagamento.Outros)
                {
                    xml += $"<xPag>{detpag.XPag}</xPag>";
                }

                xml += $"<vPag>{detpag.VPagField}</vPag>";
                xml += $"<dPag>{detpag.DPagField}</dPag>";

                if (!string.IsNullOrWhiteSpace(detpag.CNPJPag))
                {
                    xml += $"<CNPJPag>{detpag.CNPJPag}</CNPJPag>";
                    xml += $"<UFPag>{detpag.UFPag}</UFPag>";

                    if (!string.IsNullOrWhiteSpace(detpag.CNPJIF))
                    {
                        xml += $"<CNPJIF>{detpag.CNPJIF}</CNPJIF>";
                    }

#if INTEROP
                    if (detpag.TBand != (BandeiraOperadoraCartao)(-1))
#else
                    if (detpag.TBand != null)
#endif
                    {
                        xml += $"<tBand>{((int)detpag.TBand).ToString("00")}</tBand>";
                    }

                    if (!string.IsNullOrWhiteSpace(detpag.CAut))
                    {
                        xml += $"<cAut>{detpag.CAut}</cAut>";
                    }
                }

                if (!string.IsNullOrWhiteSpace(detpag.CNPJReceb))
                {
                    xml += $"<CNPJReceb>{detpag.CNPJReceb}</CNPJReceb>";
                    xml += $"<UFReceb>{detpag.UFReceb}</UFReceb>";
                }

                xml += "</detPag>";
            }

            writer.WriteRaw(xml);
        }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetPagECONF")]
    [ComVisible(true)]
#endif
    [Serializable()]
    [XmlType(AnonymousType = true, Namespace = "http://www.portalfiscal.inf.br/nfe")]
    public class DetPagECONF
    {
        private string XPagField { get; set; }

        [XmlElement("indPag")]
#if INTEROP
        public IndicadorPagamento IndPag { get; set; } = (IndicadorPagamento)(-1);
#else
        public IndicadorPagamento? IndPag { get; set; }
#endif

        [XmlElement("tPag")]
        public MeioPagamento TPag { get; set; }

        [XmlElement("xPag")]
        public string XPag
        {
            get => XPagField;
            set => XPagField = (value == null ? value : XMLUtility.UnescapeReservedCharacters(value).Truncate(60).Trim());
        }

        [XmlIgnore]
        public double VPag { get; set; }

        [XmlElement("vPag")]
        public string VPagField
        {
            get => VPag.ToString("F2", CultureInfo.InvariantCulture);
            set => VPag = Converter.ToDouble(value);
        }

        /// <summary>
        /// Data do pagamento
        /// </summary>
        [XmlIgnore]
        public DateTime DPag { get; set; }

        [XmlElement("dPag")]
        public string DPagField
        {
            get => DPag.ToString("yyyy-MM-dd");
            set => DPag = DateTime.Parse(value);
        }

        /// <summary>
        /// CNPJ transacional do pagamento. Preencher informando o CNPJ do estabelecimento onde o pagamento foi processado/transacionado/recebido quando a emissão do documento fiscal ocorrer em estabelecimento distinto
        /// </summary>
        [XmlElement("CNPJPag")]
        public string CNPJPag { get; set; }

        /// <summary>
        /// UF do CNPJ do estabelecimento onde o pagamento foi processado/transacionado/recebido.
        /// </summary>
        [XmlElement("UFPag")]
        public UFBrasil UFPag { get; set; }

        /// <summary>
        /// Preencher informando o CNPJ do estabelecimento onde o pagamento foi processado/transacionado/recebido quando a emissão do documento fiscal ocorrer em estabelecimento distinto.
        /// </summary>
        [XmlElement("CNPJIF")]
        public string CNPJIF { get; set; }

        /// <summary>
        /// Utilizar a Tabela de Códigos das Operadoras de cartão de crédito e/ou débito publicada no Portal Nacional da Nota Fiscal Eletrônica.        
        /// </summary>
        [XmlElement("tBand")]
#if INTEROP
        public BandeiraOperadoraCartao TBand { get; set; } = (BandeiraOperadoraCartao)(-1);
#else
        public BandeiraOperadoraCartao? TBand { get; set; }
#endif

        /// <summary>
        /// Identifica o número da autorização da transação da operação
        /// </summary>
        [XmlElement("cAut")]
        public string CAut { get; set; }

        /// <summary>
        /// Informar o CNPJ do estabelecimento beneficiário do pagamento
        /// </summary>
        [XmlElement("CNPJReceb")]
        public string CNPJReceb { get; set; }

        /// <summary>
        /// UF do CNPJ do estabelecimento beneficiário do pagamento.
        /// </summary>
        [XmlElement("UFReceb")]
        public UFBrasil UFReceb { get; set; }
    }

#if INTEROP
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Unimake.Business.DFe.Xml.NFe.DetEventoCancelamentoConciliacaoFinanceira")]
    [ComVisible(true)]
#endif
    [Serializable]
    [XmlRoot(ElementName = "detEvento")]
    public class DetEventoCancelamentoConciliacaoFinanceira : EventoDetalhe
    {
        /// <summary>
        /// Descrição do evento
        /// </summary>
        [XmlElement("descEvento")]
        public override string DescEvento { get; set; } = "Cancelamento Conciliação Financeira";

        /// <summary>
        /// Versão do aplicativo do Autor do Evento. 
        /// </summary>
        [XmlElement("verAplic")]
        public string VerAplic { get; set; }

        /// <summary>
        /// Informar o número do protocolo de autorização do evento de conciliação financeira da NFe/NFCe que se refere a este cancelamento
        /// </summary>
        [XmlElement("nProtEvento")]
        public string NProtEvento { get; set; }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            var xml = $@"<descEvento>{DescEvento}</descEvento>
                         <verAplic>{VerAplic}</verAplic>
                         <nProtEvento>{NProtEvento}</nProtEvento>";

            writer.WriteRaw(xml);
        }
    }

}