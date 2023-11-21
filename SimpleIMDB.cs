using MySql.Data.MySqlClient;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Text;

namespace SimpleIMDB;

public class SimpleIMDB
{
	private HttpListener server;
	private List<Task> pendingTasks;
	private bool isListening;

	private DbConnection dbc;

	public SimpleIMDB()
	{
		server = new HttpListener();
		server.Prefixes.Add("http://127.0.0.1:8080/");
		pendingTasks = new List<Task>();
		isListening = false;

		dbc = new MySqlConnection("server=127.0.0.1;uid=root;pwd=12345;database=popodb");
	}

	public void startListening()
	{
		dbc.Open();
		server.Start();
		isListening = true;

		try
		{
			while (isListening)
			{
				HttpListenerContext ctx = server.GetContext();

				pendingTasks.Add(Task.Run(() => HandleContext(ctx)));
			}
		}
		catch
		{

		}
	}

	public void stopListening()
	{
		isListening = false;
		server.Stop();

		Task.WaitAll(pendingTasks.ToArray<Task>());

		dbc.Close();
	}

	private void HandleContext(HttpListenerContext ctx)
	{
		try
		{
			HttpListenerRequest req = ctx.Request;
			using StreamReader sr = new StreamReader(req.InputStream);
			string reqBody = sr.ReadToEnd();
			NameValueCollection q = req.QueryString;
			q.Add(System.Web.HttpUtility.ParseQueryString(reqBody));

			string? feature = req.RawUrl;

			HttpListenerResponse res = ctx.Response;

			string content = "";

			if (feature.StartsWith("/viewMoviesFromActor"))
			{
				content = ViewMoviesFromActor(q);
			}
			else if (feature.StartsWith("/viewActorsFromMovie"))
			{
				content = ViewActorsFromMovie(q);
			}
			else if (feature.StartsWith("/viewMovies"))
			{
				content = ViewMovies();
			}
			else if (feature.StartsWith("/viewActors"))
			{
				content = ViewActors();
			}
			else if (feature.StartsWith("/addMovie"))
			{
				content = AddMovie(q);
			}
			else if (feature.StartsWith("/addActor"))
			{
				content = AddActor(q);
			}
			else if (feature.StartsWith("/deleteMovie"))
			{
				content = DeleteMovie(q);
			}
			else if (feature.StartsWith("/deleteActor"))
			{
				content = DeleteActor(q);
			}

			byte[] resBody = Encoding.UTF8.GetBytes(content);
			res.AddHeader("Access-Control-Allow-Origin", "*");
			res.StatusCode = (int)HttpStatusCode.OK;
			res.ContentType = "text/html; charset=utf-8;";
			res.ContentLength64 = resBody.Length;
			res.OutputStream.Write(resBody);
			res.Close();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}

	private string ViewMoviesFromActor(NameValueCollection q)
	{
		return ViewMoviesFromActor(q["actorID"]);
	}

	private string ViewMoviesFromActor(string? actorID)
	{
		string actorInfo = GetActor(actorID);

		using DbCommand cmd = dbc.CreateCommand();

		cmd.CommandText = string.Format(@"
		SELECT *
		FROM Movies
		WHERE movieID IN
		(
		  SELECT movieID
		  FROM MoviesActors
		  WHERE actorID = {0}
		)
		", actorID);

		using DbDataReader dr = cmd.ExecuteReader();

		string s = "";

		while (dr.Read())
		{
			string movieID = dr.GetString("movieID");

			s += "  <tr>";
			s += "    <td>" + movieID + "</td>\n";
			s += "    <td>" + dr.GetString("title") + "</td>\n";
			s += "    <td>" + dr.GetString("year") + "</td>\n";
			s += "    <td>" + dr.GetString("rating") + "</td>\n";
			s += "    <td><a href=\"javascript:;\" onclick=\"removeMovieFromActor(" + movieID + ", " + actorID + ");\">Remove</a></td>\n";
			s += "  </tr>";
		}

		return actorInfo + $@"
		<table border=""1"" cellpadding=""2"">
		  <tr>
		    <th>Movie ID</th>
			 <th>Title</th>
			 <th>Year</th>
			 <th>Rating</th>
			 <th>Remove</th>
		  </tr>
		  {s}
		</table>
		";
	}

	private string ViewActorsFromMovie(NameValueCollection q)
	{
		return ViewActorsFromMovie(q["movieID"]);
	}

	private string ViewActorsFromMovie(string? movieID)
	{
		string movieInfo = GetMovie(movieID);

		using DbCommand cmd = dbc.CreateCommand();

		cmd.CommandText = string.Format(@"
		SELECT *
		FROM Actors
		WHERE actorID IN
		(
		  SELECT actorID
		  FROM MoviesActors
		  WHERE movieID = {0}
		)
		", movieID);

		using DbDataReader dr = cmd.ExecuteReader();

		string s = "";

		while (dr.Read())
		{
			string actorID = dr.GetString("actorID");

			s += "  <tr>";
			s += "    <td>" + actorID + "</td>\n";
			s += "    <td>" + dr.GetString("name") + "</td>\n";
			s += "    <td>" + dr.GetString("dob") + "</td>\n";
			s += "    <td>" + dr.GetString("rating") + "</td>\n";
			s += "    <td><a href=\"javascript:;\" onclick=\"removeActorFromMovie(" + actorID + ", " + movieID + ");\">Remove</a></td>\n";
			s += "  </tr>";
		}

		return movieInfo + $@"
		<table border=""1"" cellpadding=""2"">
		  <tr>
		    <th>Actor ID</th>
			 <th>Name</th>
			 <th>Date of Birth</th>
			 <th>Rating</th>
			 <th>Remove</th>
		  </tr>
		  {s}
		</table>
		";
	}

	private string ViewMovies()
	{
		using DbCommand cmd = dbc.CreateCommand();

		cmd.CommandText = $@"
		SELECT *
		FROM Movies
		";

		using DbDataReader dr = cmd.ExecuteReader();

		string s = "";

		while (dr.Read())
		{
			string movieID = dr.GetString("movieID");

			s += "  <tr>";
			s += "    <td>" + movieID + "</td>\n";
			s += "    <td>" + dr.GetString("title") + "</td>\n";
			s += "    <td>" + dr.GetString("year") + "</td>\n";
			s += "    <td>" + dr.GetString("rating") + "</td>\n";
			s += "    <td><a href=\"javascript:;\" onclick=\"deleteMovie(" + movieID + ");\">Delete</a></td>\n";
			s += "    <td><a href=\"javascript:;\" onclick=\"viewActorsFromMovie(" + movieID + ");\">View</a></td>\n";
			s += "  </tr>";
		}

		return $@"
		<table border=""1"" cellpadding=""2"">
		  <tr>
		    <th>Movie ID</th>
			 <th>Title</th>
			 <th>Year</th>
			 <th>Rating</th>
			 <th>Delete</th>
			 <th>Actors</th>
		  </tr>
		  {s}
		</table>
		";
	}

	private string ViewActors()
	{
		using DbCommand cmd = dbc.CreateCommand();

		cmd.CommandText = $@"
		SELECT *
		FROM Actors
		";

		using DbDataReader dr = cmd.ExecuteReader();

		string s = "";

		while (dr.Read())
		{
			string actorID = dr.GetString("actorID");

			s += "  <tr>";
			s += "    <td>" + actorID + "</td>\n";
			s += "    <td>" + dr.GetString("name") + "</td>\n";
			s += "    <td>" + dr.GetString("dob") + "</td>\n";
			s += "    <td>" + dr.GetString("rating") + "</td>\n";
			s += "    <td><a href=\"javascript:;\" onclick=\"deleteActor(" + actorID + ");\">Delete</a></td>\n";
			s += "    <td><a href=\"javascript:;\" onclick=\"viewMoviesFromActor(" + actorID + ");\">View</a></td>\n";
			s += "  </tr>";
		}

		return $@"
		<table border=""1"" cellpadding=""2"">
		  <tr>
		    <th>Actor ID</th>
			 <th>Name</th>
			 <th>Date of Birth</th>
			 <th>Rating</th>
			 <th>Delete</th>
			 <th>Movies</th>
		  </tr>
		  {s}
		</table>
		";
	}

	private string AddMovie(NameValueCollection q)
	{
		using DbCommand cmd = dbc.CreateCommand();

		cmd.CommandText = string.Format(@"
		INSERT INTO Movies (title, year, rating)
		VALUES ('{0}', {1}, {2})
		", q["title"], q["year"], q["rating"]);

		Console.WriteLine(cmd.CommandText);

		cmd.ExecuteNonQuery();

		return ViewMovies();
	}

	private string AddActor(NameValueCollection q)
	{
		using DbCommand cmd = dbc.CreateCommand();

		cmd.CommandText = string.Format(@"
		INSERT INTO Actors (name, dob, rating)
		VALUES ('{0}', '{1}', {2})
		", q["name"], q["dob"], q["rating"]);

		cmd.ExecuteNonQuery();

		return ViewActors();
	}

	private string DeleteMovie(NameValueCollection q)
	{
		return DeleteMovie(q["movieID"]);
	}

	private string DeleteMovie(string? movieID)
	{
		using DbCommand cmd = dbc.CreateCommand();

		cmd.CommandText = string.Format(@"
		DELETE FROM Movies
		WHERE movieID = {0}
		", movieID);

		cmd.ExecuteNonQuery();

		return ViewMovies();
	}

	private string DeleteActor(NameValueCollection q)
	{
		return DeleteActor(q["actorID"]);
	}

	private string DeleteActor(string? actorID)
	{
		using DbCommand cmd = dbc.CreateCommand();

		cmd.CommandText = string.Format(@"
		DELETE FROM Actors
		WHERE actorID = {0}
		", actorID);

		cmd.ExecuteNonQuery();

		return ViewActors();
	}

	private string GetMovie(string? movieID)
	{
		using DbCommand cmd = dbc.CreateCommand();

		cmd.CommandText = string.Format(@"
		SELECT *
		FROM Movies
		WHERE movieID = {0}
		", movieID);

		using DbDataReader dr = cmd.ExecuteReader();

		string s = "";

		while (dr.Read())
		{
			s += "  <tr>";
			s += "    <td>" + movieID + "</td>\n";
			s += "    <td>" + dr.GetString("title") + "</td>\n";
			s += "    <td>" + dr.GetString("year") + "</td>\n";
			s += "    <td>" + dr.GetString("rating") + "</td>\n";
			s += "    <td><a href=\"javascript:;\" onclick=\"deleteMovie(" + movieID + ");\">Delete</a></td>\n";
			s += "  </tr>";
		}

		return $@"
		<table border=""1"" cellpadding=""2"">
		  <tr>
		    <th>Movie ID</th>
			 <th>Title</th>
			 <th>Year</th>
			 <th>Rating</th>
			 <th>Delete</th>
		  </tr>
		  {s}
		</table>
		";
	}

	private string GetActor(string? actorID)
	{
		using DbCommand cmd = dbc.CreateCommand();

		cmd.CommandText = string.Format(@"
		SELECT *
		FROM Actors
		WHERE actorID = {0}
		", actorID);

		using DbDataReader dr = cmd.ExecuteReader();

		string s = "";

		while (dr.Read())
		{
			s += "  <tr>";
			s += "    <td>" + actorID + "</td>\n";
			s += "    <td>" + dr.GetString("name") + "</td>\n";
			s += "    <td>" + dr.GetString("dob") + "</td>\n";
			s += "    <td>" + dr.GetString("rating") + "</td>\n";
			s += "    <td><a href=\"javascript:;\" onclick=\"deleteActor(" + actorID + ");\">Delete</a></td>\n";
			s += "  </tr>";
		}

		return $@"
		<table border=""1"" cellpadding=""2"">
		  <tr>
		    <th>Actor ID</th>
			 <th>Name</th>
			 <th>Date of Birth</th>
			 <th>Rating</th>
			 <th>Delete</th>
		  </tr>
		  {s}
		</table>
		";
	}
}
