async function feature(name, req)
{
  let resBody = "";

  try
  {
    let res = await fetch(`http://localhost:8080/${name}`, req);
    resBody = await res.text();
  }
  catch(ex)
  {
    resBody = ex.message;
  }

  let div = document.getElementById('Content');

  div.innerHTML = resBody;
}

function showAddMovieForm()
{
  let div = document.getElementById('Content');
  div.innerHTML =`
  <form onsubmit="event.preventDefault(); addMovie(this);">
    <label for="title">Title: </label>
    <input id="title" name="title" type="text" value="Babylon">
    <br>
    <label for="year">Year: </label>
    <input id="year" name="year" type="number" value="2020" min="1900">
    <br>
    <label for="rating">Rating: </label>
    <input id="rating" name="rating" type="number" value="9.8" min="0" max="10" step="0.1">
    <input type="submit">
  </form>
  `;
}

function showAddActorForm()
{
  let div = document.getElementById('Content');
  div.innerHTML =`
  <form onsubmit="event.preventDefault(); addActor(this);">
    <label for="name">Name: </label>
    <input id="name" name="name" type="text" value="Chris">
    <br>
    <label for="dob">Date of Birth: </label>
    <input id="dob" name="dob" type="date" value="1990-01-01" min="1800-01-01">
    <br>
    <label for="rating">Rating: </label>
    <input id="rating" name="rating" type="number" value="9.8" min="0" max="10" step="0.1">
    <input type="submit">
  </form>
  `;
}

function addMovie(form)
{
  if(!window.confirm("Are you sure that you want to add this movie?"))
  {
    return;
  }

  const req =
  {
    method: "post",
    body: new URLSearchParams(new FormData(form)).toString()
  };

  feature("addMovie", req);
}

function addActor(form)
{
  if(!window.confirm("Are you sure that you want to add this actor?"))
  {
    return;
  }

  console.log(new URLSearchParams(new FormData(form)).toString());

  let req =
  {
    method: "post",
    body: new URLSearchParams(new FormData(form)).toString()
  };

  feature("addActor", req);
}

function deleteMovie(movieID)
{
  if(!window.confirm("Are you sure that you want to delete this movie?"))
  {
    return;
  }

  let req =
  {
    method: "post",
    body: "movieID=" + movieID
  };

  feature("deleteMovie", req);
}

function deleteActor(actorID)
{
  if(!window.confirm("Are you sure that you want to delete this actor?"))
  {
    return;
  }

  let req =
  {
    method: "post",
    body: "actorID=" + actorID
  };

  feature("deleteActor", req);
}

function viewMoviesFromActor(actorID)
{
  feature("viewMoviesFromActor/?actorID=" + actorID);
}

function viewActorsFromMovie(movieID)
{
  feature("viewActorsFromMovie/?movieID=" + movieID);
}
