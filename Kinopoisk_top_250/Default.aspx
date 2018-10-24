<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Kinopoisk_top_250.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <link rel="stylesheet" href ="styles.css" />
    <link rel="stylesheet" href ="table_cell_styles.css" />
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="height: 143px">
            <asp:Button ID="BParse" runat="server" Text="Парсим страницу Кинопоиска" OnClick="BParse_Click" />
            <asp:Button ID="BToSql" runat="server" Text="Записать топ 250 в SQL" Enabled="False" OnClick="BToSql_Click" />
            <asp:Button ID="BLoadSql" runat="server" Text="Загрузить топ 250 из SQL" OnClick="BLoadSql_Click" />
            <asp:Button ID="BFind" runat="server" Text="Найти:" Enabled="False" OnClick="BFind_Click" />
            <asp:Label ID="Label2" runat="server" Text="от: "></asp:Label>
            <asp:DropDownList ID="CBFrom" runat="server" Enabled="False">
            </asp:DropDownList>
            <asp:Label ID="Label3" runat="server" Text="до: "></asp:Label>
            <asp:DropDownList ID="CBTo" runat="server" Enabled="False">
            </asp:DropDownList>
            <br />
            <asp:Label ID="LExp" runat="server"></asp:Label>
            <br />
            <asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
        </div>
    </form>
</body>
</html>
