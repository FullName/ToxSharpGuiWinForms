
using System;
using System.Drawing;
using WinForms = System.Windows.Forms;

using ToxSharpBasic;

namespace ToxSharpWinForms
{
	public class ToxSharpWinForms : Interfaces.IUIFactory
	{
		protected ToxSharpWinFormsMain wnd;

		public ToxSharpWinForms()
		{
		}

		public Interfaces.IUIReactions Create()
		{
			wnd = new ToxSharpWinFormsMain();
			return wnd;
		}

		public void Quit()
		{
			WinForms.Application.Exit();
		}
	}

	public class ToxSharpWinFormsMain : WinForms.Form, Interfaces.IUIReactions
	{
		// Interfaces.IUIReactions
		public void ToxDo(Interfaces.CallToxDo calltoxdo, IntPtr tox)
		{
			if (IsHandleCreated)
				Invoke(calltoxdo, tox);
			else
				calltoxdo(tox);
		}

		internal delegate void ConnectStateDelegate(bool state, string text);

		internal void ConnectStateInvokee(bool state, string text)
		{
			connectstate.Checked = state;
			connectstate.Text = text;
		}

		public void ConnectState(bool state, string text)
		{
			ConnectStateDelegate connectstate = new ConnectStateDelegate(ConnectStateInvokee);
			if (IsHandleCreated)
				Invoke(connectstate, state, text);
			else
				connectstate(state, text);
		}

	/************************************************************************/
	/************************************************************************/
	/************************************************************************/

		public void TitleUpdate(string name, string ID)
		{
			if ((name != null) && (ID != null))
				Text = "Tox# - " + name + " [" + ID + "]";
		}

		public void TreeAddSub(TypeIDTreeNode typeid, TypeIDTreeNode parenttypeid)
		{
			HolderTreeNode grandparent = TreeParent(parenttypeid);
			if (grandparent != null)
			{
				foreach(WinForms.TreeNode wfparent in grandparent.Nodes)
				{
					HolderTreeNode parent = wfparent as HolderTreeNode;
					if (parent.typeid == parenttypeid)
					{
						parent.Nodes.Add(new HolderTreeNode(typeid));
						parent.Expand();
						grandparent.Expand();
						break;
					}
				}

				TreeUpdateSub(typeid, parenttypeid);
			}
		}

		public void TreeDelSub(TypeIDTreeNode typeid, TypeIDTreeNode parenttypeid)
		{
			HolderTreeNode grandparent = TreeParent(parenttypeid);
			if (grandparent != null)
			{
				foreach(WinForms.TreeNode wfparent in grandparent.Nodes)
				{
					HolderTreeNode parent = wfparent as HolderTreeNode;
					if (parent.typeid == parenttypeid)
					{
						foreach(WinForms.TreeNode wfcandidate in parent.Nodes)
						{
							HolderTreeNode candidate = wfcandidate as HolderTreeNode;
							if (candidate.typeid == typeid)
							{
								parent.Nodes.Remove(candidate);
								break;
							}
						}

						break;
					}
				}

				TreeUpdateSub(typeid, parenttypeid);
			}
		}

		public void TreeUpdateSub(TypeIDTreeNode typeid, TypeIDTreeNode parenttypeid)
		{
			if (parenttypeid == null)
				return;

			if (typeid != null)
			{
				HolderTreeNode grandparent = TreeParent(parenttypeid);
				foreach(HolderTreeNode parent in grandparent.Nodes)
					if (parent.typeid == parenttypeid)
					{
						foreach(HolderTreeNode child in parent.Nodes)
							if (child.typeid == typeid)
							{
								child.Text = child.typeid.Text();
								if ((typeid.entryType == TypeIDTreeNode.EntryType.Friend) && (child.Text == ""))
									child.Text = "(no name)";
								child.ToolTipText = child.typeid.TooltipText();
								break;
							}

						break;
					}
			}
			else
			{
				HolderTreeNode grandparent = TreeParent(parenttypeid);
				foreach(HolderTreeNode parent in grandparent.Nodes)
					if (parent.typeid == parenttypeid)
					{
						foreach(HolderTreeNode child in parent.Nodes)
						{
							child.Text = child.typeid.Text();
							if ((child.typeid.entryType == TypeIDTreeNode.EntryType.Friend) && (child.Text == ""))
								child.Text = "(no name)";
							child.ToolTipText = child.typeid.TooltipText();
						}

						break;
					}
			}

			people.Refresh();
		}

		public void TreeAdd(TypeIDTreeNode typeid)
		{
			HolderTreeNode parent = TreeParent(typeid);
			if (parent != null)
			{
				parent.Nodes.Add(new HolderTreeNode(typeid));
				uiactions.PrintDebug("Adding treenode: " + typeid.entryType + "." + typeid.ids());
				parent.Expand();
				TreeUpdate(typeid);
			}
		}

		public void TreeDel(TypeIDTreeNode typeid)
		{
			HolderTreeNode parent = TreeParent(typeid);
			if (parent == null)
				return;

			foreach(HolderTreeNode child in parent.Nodes)
				if (child.typeid == typeid)
				{
					parent.Nodes.Remove(child);
					if (parent.Nodes.Count == 0)
						people.Nodes.Remove(parent);

					break;
				}

			people.Refresh();
			TreeUpdate(null);
		}

		public void TreeUpdate(TypeIDTreeNode typeid)
		{
			if (typeid != null)
			{
				HolderTreeNode parent = TreeParent(typeid);
				foreach(HolderTreeNode child in parent.Nodes)
					if (child.typeid == typeid)
					{
						child.Text = child.typeid.Text();
						if ((child.typeid.entryType == TypeIDTreeNode.EntryType.Friend) && (child.Text == ""))
							child.Text = "(no name)";
						child.ToolTipText = child.typeid.TooltipText();
					    break;
					}
			}
			else
				foreach(HolderTreeNode parent in people.Nodes)
					foreach(HolderTreeNode child in parent.Nodes)
					{
						child.Text = child.typeid.Text();
						if ((child.typeid.entryType == TypeIDTreeNode.EntryType.Friend) && (child.Text == ""))
							child.Text = "(no name)";
						child.ToolTipText = child.typeid.TooltipText();
					}

			people.Refresh();
		}

		// external: clipboard
		public void ClipboardSend(string text)
		{
			WinForms.Clipboard.SetText(text);
		}

		// right side: multi-tab
		public bool CurrentTypeID(out Interfaces.SourceType type, out System.UInt32 id)
		{
			if (pages.TabIndex > 0)
			{
				TabbedPage page = pages.SelectedTab as TabbedPage;
				if (page != null)
				{
					UInt16 ids;
					page.TypeID(out type, out ids);
					id = ids;
					return true;
				}
			}

			type = Interfaces.SourceType.System;
			id = 0;
			return false;
		}

		public void TextAdd(Interfaces.SourceType type, UInt32 id32, string source, string text)
		{
			if (id32 > UInt16.MaxValue)
				throw new ArgumentOutOfRangeException();

			UInt16 id = (UInt16)id32;

			// TODO: multiple rows if source or text contain newlines
			// TODO: split across multiple rows, mark as continuation, in resize recalc

			WinForms.TabPage main = pages.TabPages[0];
			WinForms.ListView output = main.Controls[0] as WinForms.ListView;
			WinForms.ListViewItem item = output.Items.Add(source);
			item.SubItems.Add(text);

			object[] olist = new object[3];
			olist[0] = type;
			olist[1] = id;
			olist[2] = DateTime.Now;
			item.Tag = olist;
			output.EnsureVisible(item.Index);

			if ((type == Interfaces.SourceType.Friend) ||
			    (type == Interfaces.SourceType.Group))
				foreach(WinForms.TabPage wfpage in pages.TabPages)
				{
					TabbedPage page = wfpage as TabbedPage;
					if (page.Is(type, id))
					{
						output = wfpage.Controls[0] as WinForms.ListView;
						item = output.Items.Add(source);
						item.SubItems.Add(text);
						output.EnsureVisible(item.Index);
					}
				}
		}

		// create and execute a popup menu
		public void PopupMenuDo(object parent, Point position, Interfaces.PopupEntry[] entries)
		{
			WinForms.Control parentcontrol = parent as WinForms.Control;
			if ((parentcontrol == null) || (entries.Length == 0))
				return;

			for(int i = 0; i < entries.Length; i++)
				if (entries[i].parent >= 0)
				{
				    // first parents, then children, no self-ref
					if (entries[i].parent >= i)
						return;

					// out of bounds
				    if (entries[i].parent >= entries.Length)
						return;
				}

			// generate lowest children
			// generate their parents
			// until all is tied in
			WinForms.ContextMenu[] menus = new WinForms.ContextMenu[entries.Length];
			WinForms.MenuItem[] items = new WinForms.MenuItem[entries.Length];

			for(int i = 0; i < entries.Length; i++)
			{
				Interfaces.PopupEntry entry = entries[i];

				items[i] = new WinForms.MenuItem(entry.title, entry.handle);
				items[i].Name = entry.action;
				items[i].Visible = true;

				if (entry.parent >= 0)
				{
					if (menus[entry.parent] == null)
					{
						menus[entry.parent] = new WinForms.ContextMenu();
						items[entry.parent].Enabled = false;
						items[entry.parent].MergeMenu(menus[entry.parent]);
					}

					menus[entries[i].parent].MenuItems.Add(items[i]);
				}
			}

			WinForms.ContextMenu menu = new WinForms.ContextMenu();

			for(int i = 0; i < entries.Length; i++)
				menu.MenuItems.Add(items[i]);

			menu.Show(parentcontrol, position);
		}

		public string PopupMenuAction(object o, System.EventArgs args)
		{
			WinForms.MenuItem item = o as WinForms.MenuItem;
			if (item != null)
				return item.Name;
			else
				return null;
		}

		// ask user for two strings: the ID and a message for a friend-invite
		public bool AskIDMessage(string message, string name1, string name2, out string input1, out string input2)
		{
			// TODO: dialog
			TextAdd(Interfaces.SourceType.Debug, 0, "DEBUG", "Not yet implemented. Use the command line. Sorry!");

			input1 = null;
			input2 = null;
			return false;
		}

		// close down application
		public void Quit()
		{
			ClosedHandler(null, null);
		}

		private WinForms.SplitContainer splitcontainer;

		private WinForms.CheckBox connectstate;
		private WinForms.TreeView people;

		private WinForms.TabControl pages;
		private WinForms.TextBox  input;

		private const int WidthMin = 400;
		private const int HeightMin = 300;
		private const int LeftWidth = 160;

		public ToxSharpWinFormsMain()
		{
			uiactions = null;

			SetClientSizeCore(WidthMin + 15, HeightMin + 15);

			int RightWidth = WidthMin - LeftWidth;

			splitcontainer = new WinForms.SplitContainer();
			splitcontainer.Width = WidthMin;
			splitcontainer.Height = HeightMin;
			splitcontainer.Panel2.Resize += ResizeHandler;

			Controls.Add(splitcontainer);


			connectstate = new WinForms.CheckBox();
			connectstate.Text = "Disconnected.";
			connectstate.Enabled = false;

			connectstate.Left = 5;
			connectstate.Width = LeftWidth;

			splitcontainer.Panel1.Controls.Add(connectstate);


			people = new WinForms.TreeView();
			people.ShowNodeToolTips = true;
			people.MouseClick += TreeViewMouseSingleClickHandler;
			people.MouseDoubleClick += TreeViewMouseDoubleClickHandler;
			people.KeyUp += TreeViewKeyUp;
			people.MouseUp += TreeViewMouseUp;

			people.Left = 5;
			people.Top = connectstate.Bottom + 5;
			people.Width = LeftWidth;
			people.Height = HeightMin - connectstate.Height;

			splitcontainer.Panel1.Controls.Add(people);


			pages = new WinForms.TabControl();
			pages.Alignment = WinForms.TabAlignment.Bottom;
			pages.Deselecting += PageDeselecting;
			pages.Selecting += PageSelecting;
			pages.SelectedIndexChanged += ResizeHandler;

			pages.Left = 5;
			pages.Top = 5;
			pages.Width = RightWidth;
			pages.Height = HeightMin;

			splitcontainer.Panel2.Controls.Add(pages);

			input = new WinForms.TextBox();
			input.Multiline = false;
			input.KeyPress += TextBoxKeyPressHandler;

			input.Left = 5;
			input.Width = RightWidth;
			input.Top = pages.Bottom + 5;

			splitcontainer.Panel2.Controls.Add(input);


			PageAdd(null, "Main");

			Resize += ResizeHandler;
			Closed += ClosedHandler;

			// TODO: Focus => tb
			input.Select();
		}

		private void TreeViewKeyUp (object sender, WinForms.KeyEventArgs e)
		{
			if ((e.Modifiers == WinForms.Keys.None) &&
			    (e.KeyData == WinForms.Keys.Return))
			{
				WinForms.TreeView view = sender as WinForms.TreeView;
				if (view != null)
				{
					WinForms.TreeNode node = view.SelectedNode;
					if (node != null)
					{
						HolderTreeNode holder = node as HolderTreeNode;
						if (holder != null)
							PageAdd(holder.typeid, holder.typeid.Text());
					}
				}
			}
			else
				TextAdd(Interfaces.SourceType.Debug, 0, "DEBUG", "KEYUP: " + e.KeyValue);
		}

		protected Interfaces.IUIActions uiactions;

		public void Init(Interfaces.IUIActions uiactions)
		{
			this.uiactions = uiactions;
		}

		protected int col1 = -1;
		protected int col2 = -1;

		private void PageDeselecting(object o, WinForms.TabControlCancelEventArgs e)
		{
			TabbedPage page = pages.SelectedTab as TabbedPage;
			WinForms.ListView view = page.Controls[0] as WinForms.ListView;
			col1 = view.Columns[0].Width;
			col2 = view.Columns[1].Width;
		}

		private void PageSelecting(object o, WinForms.TabControlCancelEventArgs e)
		{
			TabbedPage page = pages.SelectedTab as TabbedPage;
			WinForms.ListView view = page.Controls[0] as WinForms.ListView;
			if (col1 > 0)
				view.Columns[0].Width = col1;
			if (col2 > 0)
				view.Columns[1].Width = col2;
		}

		public struct UIData
		{
			public int x, y, w, h, spl, c1, c2;
		}

		public void Run(string uistate)
		{
			// TODO: parse uistate for window position
			UIData uidata;
			try
			{
				uidata = SerializeFromString<UIData>(uistate);

				Show();

				Location.X = uidata.x;
				Location.Y = uidata.y;
				Width = uidata.w;
				Height = uidata.h;
				splitcontainer.SplitterDistance = uidata.spl;
				col1 = uidata.c1;
				col2 = uidata.c2;


				ResizeHandler(null, null);
			}
			catch
			{
			}

			WinForms.Application.Run(this);
		}

		internal static string SerializeToString(object obj)
		{
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
			using (System.IO.StringWriter writer = new System.IO.StringWriter())
			{
				serializer.Serialize(writer, obj);
				return writer.ToString();
			}
		}

		internal static T SerializeFromString<T>(string xml)
		{
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

			using (System.IO.StringReader reader = new System.IO.StringReader(xml))
			{
				return (T)serializer.Deserialize(reader);
			}
		}

		private void ClosedHandler(object sender, EventArgs e)
		{
			if ((col1 == -1) || (col2 == -1))
				PageDeselecting(null, null);

			UIData uidata;
			uidata.x = Location.X;
			uidata.y = Location.Y;
			uidata.w = splitcontainer.Width;
			uidata.h = Height;
			uidata.spl = splitcontainer.SplitterDistance;
			uidata.c1 = col1;
			uidata.c2 = col2;

			string uistate = SerializeToString(uidata);
			uiactions.QuitPrepare(uistate);
			WinForms.Application.Exit();
		}

		private void ResizeHandler(object sender, EventArgs e)
		{
			int HeightNow = Height - 48;
			splitcontainer.Height = HeightNow;
			splitcontainer.Width = Width - 25;

			people.Height = HeightNow - connectstate.Height;
			pages.Height  = HeightNow - (input.Height + 12);
			input.Top     = pages.Bottom + 5;

			people.Width = splitcontainer.Panel1.Width - 5;
			pages.Width = splitcontainer.Panel2.Width - 5;
			input.Width = splitcontainer.Panel2.Width - 5;

			PageUpdate();
		}

		class TabbedPage : WinForms.TabPage
		{
			protected Interfaces.SourceType type;
			protected UInt16 id;

			protected TabbedPage(WinForms.TabControl pages, WinForms.TextBox input, Interfaces.SourceType type, UInt16 id, string title, int col1, int col2) : base(title)
			{
				this.type = type;
				this.id = id;

				int RightWidth = pages.Width;

				WinForms.ListView output = new WinForms.ListView();
				output.View = WinForms.View.Details;
				output.Scrollable = true;
				output.Columns.Add("Source", col1 > 0 ? col1 : 60);
				output.Columns.Add("Text", col2 > 0 ? col2 : RightWidth - 24 - 60);
				output.HeaderStyle = WinForms.ColumnHeaderStyle.Nonclickable;

				output.Width = RightWidth;
				output.Height = HeightMin - input.Height - 28;

				Controls.Add(output);
			}

			public bool Is(Interfaces.SourceType type, UInt16 id)
			{
				return ((this.type == type) && (this.id == id));
			}

			public void TypeID(out Interfaces.SourceType type, out UInt16 id)
			{
				type = this.type;
				id = this.id;
			}

			public static void PageAdd(WinForms.TabControl pages, WinForms.TextBox input, TypeIDTreeNode typeid, string title, int col1, int col2)
			{
				Interfaces.SourceType type = Interfaces.SourceType.System;
				UInt16 id = 0;
				if (pages.Controls.Count > 0)
				{
					if (typeid == null)
						return;

					if (typeid.entryType == TypeIDTreeNode.EntryType.Friend)
						type = Interfaces.SourceType.Friend;
					else if (typeid.entryType == TypeIDTreeNode.EntryType.Group)
						type = Interfaces.SourceType.Group;
					else
						return;

					foreach(WinForms.TabPage wfpage in pages.TabPages)
					{
						TabbedPage page = wfpage as TabbedPage;
						if (page != null)
							if (page.Is(type, typeid.ids()))
							{
								pages.SelectedTab = page;
								return;
							}
					}

					id = typeid.ids();
				}

				TabbedPage tabbedpage = new TabbedPage(pages, input, type, id, title, col1, col2);
				pages.Controls.Add(tabbedpage);
				pages.SelectedTab = tabbedpage;

				try
				{
					WinForms.ListView output = tabbedpage.Controls[0] as WinForms.ListView;

					TabbedPage tabbedpage0 = pages.TabPages[0] as TabbedPage;
					WinForms.ListView output0 = tabbedpage0.Controls[0] as WinForms.ListView;
					for(int i = 0; i < output0.Items.Count; i++)
					{
						try
						{
							WinForms.ListViewItem item0 = output0.Items[i];
							object[] olist = item0.Tag as object[];
							Interfaces.SourceType type0 = (Interfaces.SourceType)olist[0];
							UInt16 id0 = (UInt16)olist[1];
							if ((type0 == type) && (id0 == id))
							{
								string source = item0.Text;
								string text = item0.SubItems[1].Text;
								WinForms.ListViewItem item = output.Items.Add(source);
								item.SubItems.Add(text);
								item.Tag = item0.Tag;
							}
						}
						catch (Exception e)
						{
							System.Console.WriteLine("PageAdd::Internal::CopyItem: " + e.Message);
						}
					}
				}
				catch (Exception e)
				{
					System.Console.WriteLine("PageAdd::Internal::CopyItems: " + e.Message);
				}
			}
		}

		protected void PageAdd(TypeIDTreeNode typeid, string title)
		{
			if (pages.Controls.Count > 0)
				if ((typeid.entryType != TypeIDTreeNode.EntryType.Friend) &&
			        (typeid.entryType != TypeIDTreeNode.EntryType.Group))
					return;

			TabbedPage.PageAdd(pages, input, typeid, title, col1, col2);
		}

		protected void PageUpdate()
		{
			WinForms.TabPage page = pages.SelectedTab;

			WinForms.ListView output = page.Controls[0] as WinForms.ListView;
			output.Width = pages.Width;
			output.Height = pages.Height - 24;
		}

		void TreeViewMouseUp(object sender, WinForms.MouseEventArgs e)
		{
			// non-node area handling must be done here
			TextAdd(Interfaces.SourceType.Debug, 0, "DEBUG", "MouseUp@" + e.X + ":" + e.Y);
			TreeViewMouseClick(sender, e.Location, e.Button, Interfaces.Click.Single);
		}

		void TreeViewMouseClick(object sender, Point location, WinForms.MouseButtons wfbutton, Interfaces.Click click)
		{
			string dbgmsg = "TVNMC: ";

			WinForms.TreeView view = sender as WinForms.TreeView;
			WinForms.TreeNode node = view.GetNodeAt(location);
			HolderTreeNode holder = node as HolderTreeNode;
			TypeIDTreeNode typeid = null;
			if (holder != null)
			{
				typeid = holder.typeid;
				dbgmsg += "[^" + typeid.entryType.ToString() + ":" + typeid.id + "] ";
			}
			else if (node != null)
				return;

			Interfaces.Button button = Interfaces.Button.None;
			switch(wfbutton)
			{
				case WinForms.MouseButtons.Left:
					button = Interfaces.Button.Left;
					dbgmsg += "L";
					break;
				case WinForms.MouseButtons.Middle:
					button = Interfaces.Button.Middle;
					dbgmsg += "M";
					break;
				case WinForms.MouseButtons.Right:
					button = Interfaces.Button.Right;
					dbgmsg += "R";
					break;
			}

			dbgmsg += click.ToString();
			TextAdd(Interfaces.SourceType.Debug, 0, "DEBUG", dbgmsg);

			if (typeid == null)
				uiactions.TreePopup(this, Location, typeid, button, click);
			else
				uiactions.TreePopup(sender, Location, typeid, button, click);
		}

		void TreeViewMouseSingleClickHandler(object sender, WinForms.MouseEventArgs e)
		{
			// TreeViewMouseClick(sender, e.Location, e.Button, Popups.Click.Single);
		}

		void TreeViewMouseDoubleClickHandler(object sender, WinForms.MouseEventArgs e)
		{
			// TreeViewMouseClick(sender, e.Location, e.Button, Popups.Click.Double);
			WinForms.TreeView view = sender as WinForms.TreeView;
			if (view == null)
				return;

			WinForms.TreeNode node = view.GetNodeAt(e.Location);
			HolderTreeNode holder = node as HolderTreeNode;
			if (holder == null)
				return;

			TypeIDTreeNode typeid = holder.typeid;
			PageAdd(typeid, typeid.Text());
		}

		void TextBoxKeyPressHandler(object sender, WinForms.KeyPressEventArgs e)
		{
			Interfaces.InputKey key = Interfaces.InputKey.None;
			switch((WinForms.Keys)e.KeyChar)
			{
				case WinForms.Keys.Up:
				    key = Interfaces.InputKey.Up;
					break;
				case WinForms.Keys.Down:
				    key = Interfaces.InputKey.Down;
					break;
				case WinForms.Keys.Tab:
				    key = Interfaces.InputKey.Tab;
					break;
				case WinForms.Keys.Return:
				    key = Interfaces.InputKey.Return;
					break;
				default:
					return;
			}

			uiactions.InputLine(input.Text, key);
		}

		protected HolderTreeNode[] headers;

		protected HolderTreeNode TreeParent(TypeIDTreeNode typeid)
		{
			if (headers == null)
			{
				int idmax = -1;
				Array valueAry = Enum.GetValues(typeof(TypeIDTreeNode.EntryType));
				foreach (int idenum in valueAry)
					if (idenum > idmax)
						idmax = idenum;

				idmax++;
				headers = new HolderTreeNode[idmax];
			}

			UInt16 id = (UInt16)typeid.entryType;
			if (headers[id] != null)
				return headers[id];

			HeaderTreeNode header = new HeaderTreeNode(typeid.entryType);
			HolderTreeNode holder = new HolderTreeNode(header);
			holder.Text = header.Text();
			if ((typeid.entryType == TypeIDTreeNode.EntryType.Friend) && (holder.Text == ""))
				holder.Text = "(no name)";
			holder.ToolTipText = header.TooltipText();
			headers[id] = holder;

			// find the first next header to insert over
			for(int next = id + 1; next < headers.Length; next++)
				if (headers[next] != null)
				{
					people.Nodes.Insert(headers[next].Index, holder);
					return holder;
				}

			people.Nodes.Add(holder);
			return holder;
		}
	}

	public class HolderTreeNode : WinForms.TreeNode
	{
		public TypeIDTreeNode typeid;

		public HolderTreeNode(TypeIDTreeNode typeid)
		{
			this.typeid = typeid;
		}
	}
}
