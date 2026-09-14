/// Las cuatro situaciones en las que puede estar una pantalla que carga datos. Tenerlas
/// enumeradas obliga a que ninguna pantalla se olvide de dibujar el error o el vacío.
enum UiState { idle, loading, success, failure }
