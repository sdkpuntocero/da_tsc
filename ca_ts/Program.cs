using NReco.VideoConverter;
using NReco.VideoInfo;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace ca_ts
{
    internal class Program
    {
        private static string str_Id = null;

        private static void Main(string[] args)
        {
            string str_ffmpg = null;
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName == "ffmpeg.exe" || p.ProcessName == "ffmpeg")
                {
                    str_ffmpg = p.ProcessName;
                    break;
                }
                else
                {
                }
            }

            if (str_ffmpg == "ffmpeg")
            {
            }
            else
            {
                recorre_carpetas();
            }
        }

        private static void recorre_carpetas()
        {
            string ext_all, ext_mp4, ext_asf, ext_wmv, usr_ini, clv_ini;
            Guid id_ctrl = Guid.Empty;
            DateTime dt_fr, dt_frc;
            ext_mp4 = ".mp4";
            ext_asf = ".asf";
            ext_wmv = ".wmv";

            double differenceInMinutes = 0;

            using (var md_ft = new bd_tsEntities())
            {

                var i_rv = (from c in md_ft.inf_ruta_videos
                            select c).ToList();
                if (i_rv.Count == 0)
                {
                    Console.WriteLine("Sin rutas de videos, favor de agregar");
                }
                else
                {
                    Console.WriteLine("Ejecutando...!!!");
                    foreach (var f_rv in i_rv)
                    {
                        DirectoryInfo ruta_compartida = new DirectoryInfo(f_rv.desc_ruta_ini);
                        usr_ini = f_rv.ruta_user_ini;
                        clv_ini = f_rv.ruta_pass_ini;


                         DirectoryInfo ruta_destino = new DirectoryInfo(f_rv.desc_ruta_fin);
                        int id_rv = f_rv.id_ruta_videos;
                        var networkPath = ruta_compartida.ToString();
                        var credentials = new NetworkCredential(usr_ini, clv_ini);
                        try
                        {
                            using (new networkconnection(networkPath, credentials))
                            {

                                foreach (DirectoryInfo dir_c_f in ruta_compartida.GetDirectories())
                                {
                                    id_ctrl = Guid.NewGuid();
                                    verifica_carpeta(dir_c_f, ruta_destino, id_rv, 1, id_ctrl);

                                    foreach (DirectoryInfo dir_c_ff in dir_c_f.GetDirectories())
                                    {
                                        DirectoryInfo ruta_subdestino = new DirectoryInfo(ruta_destino + "\\" + dir_c_ff.Parent);
                                        verifica_carpeta(dir_c_ff, ruta_subdestino, id_rv, 2, id_ctrl);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Sin acceso a la ruta de red, {0}", e.ToString());
                            Console.WriteLine("Favor de revisar o contactar a soporte");
                        }
                    }

                    //dt_fr = DateTime.Parse(i_ft[0].horario.ToString());
                    //dt_frc = DateTime.Now;

                    //differenceInMinutes = (double)(dt_frc - dt_fr).Minutes;
                    //if (dt_frc >= dt_fr && differenceInMinutes == 0.0)
                    //{
                    //    Console.WriteLine("Inicia proceso de carga");
                    //}
                    //else // No Coincide la hora para la carga
                    //{
                    //    Console.WriteLine("No Coincide la hora para la carga");


                    //}
                }

                //var i_ft = (from c in md_ft.inf_fecha_transformacion
                //            select c).ToList();

                //if (i_ft.Count == 0)
                //{
                //    Console.WriteLine("Sin fecha de transformación, favor de agregar");
                //}
                //else
                //{

                //}
            }
        }

        private static void verifica_carpeta(DirectoryInfo dir_c_f, DirectoryInfo ruta_destino, int id_rv, int t_ss, Guid id_ctrl)
        {
            string ext_all, ext_mp4, ext_asf, ext_wmv, ext_pdf, usr_ini, clv_ini;

            ext_mp4 = ".mp4";
            ext_asf = ".asf";
            ext_wmv = ".wmv";
            ext_pdf = ".pdf";

            int est_matID = 0;

            Guid id_em = Guid.Empty;

            var lis_wmv = dir_c_f.GetFiles("*wmv");
            if (lis_wmv.Length > 0)
            {
                using (var edm_master = new bd_tsEntities())
                {
                    var i_master = (from c in edm_master.inf_master_jvl
                                    where c.sesion == dir_c_f.Name
                                    select c).ToList();

                    if (t_ss == 1)
                    {
                        if (i_master.Count == 0)
                        {
                            inf_master_jvl infMaster = new inf_master_jvl()
                            {
                                id_control_exp = id_ctrl,
                                sesion = dir_c_f.Name,
                                titulo = dir_c_f.Name,
                                err_carga = "Ninguno",
                                id_estatus_exp = 1,
                                id_estatus_qa = 1,
                                id_ruta_videos = id_rv,
                                fecha_registro = DateTime.Now
                            };
                            edm_master.inf_master_jvl.Add(infMaster);
                            edm_master.SaveChanges();

                            foreach (FileInfo f_c_f in dir_c_f.GetFiles("*wmv"))
                            {
                                DirectoryInfo di_destino = new DirectoryInfo(ruta_destino + "\\" + dir_c_f.Name);

                                if (di_destino.Exists == true)
                                {
                                }
                                else
                                {
                                    di_destino.Create();
                                }

                                File.Copy(f_c_f.FullName, di_destino + "\\" + f_c_f.Name, true);
                                id_em = Guid.NewGuid();

                                FFProbe ffProbe = new FFProbe();
                                var videoInfo = ffProbe.GetMediaInfo(di_destino + "\\" + f_c_f.Name.ToString());
                                string f_d = videoInfo.Duration.Hours + ":" + videoInfo.Duration.Minutes + ":" + videoInfo.Duration.Seconds;

                                var g_media = new inf_exp_mat
                                {
                                    id_exp_mat = id_em,
                                    ruta_archivo = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_wmv, ext_mp4),
                                    ruta_ext = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_wmv, ".pdf"),
                                    duracion = f_d,
                                    nom_archivo = f_c_f.Name.Replace(ext_wmv, ""),
                                    id_est_mat = 1,
                                    id_control_exp = id_ctrl,
                                    fecha_registro = DateTime.Now,
                                };
                                edm_master.inf_exp_mat.Add(g_media);
                                edm_master.SaveChanges();

                                try
                                {
                                    FFMpegConverter ffMpegConverter = new FFMpegConverter();
                                    ffMpegConverter.ConvertMedia(di_destino + "\\" + f_c_f.Name, di_destino + "\\" + f_c_f.Name.Replace(ext_wmv, ext_mp4), Format.mp4);
                                    File.Delete(di_destino + "\\" + f_c_f.Name);
                                    est_matID = 2;

                                    var a_media = (from c in edm_master.inf_exp_mat
                                                   where c.id_exp_mat == id_em
                                                   select c).FirstOrDefault();

                                    a_media.id_est_mat = est_matID;
                                    edm_master.SaveChanges();
                                }
                                catch
                                {
                                    est_matID = 3;

                                    var a_media = (from c in edm_master.inf_exp_mat
                                                   where c.id_exp_mat == id_em
                                                   select c).FirstOrDefault();

                                    a_media.id_est_mat = est_matID;
                                    edm_master.SaveChanges();
                                }
                            }
                        }
                        else
                        {

                            id_ctrl = i_master[0].id_control_exp;
                            foreach (FileInfo f_c_f in dir_c_f.GetFiles("*wmv"))
                            {
                                string f_f = ruta_destino + "\\" + dir_c_f.Name + "\\" + f_c_f.Name.Replace(ext_wmv, ext_mp4);
                                var i_em_f = (from c in edm_master.inf_exp_mat
                                              where c.ruta_archivo == f_f
                                              select c).ToList();

                                if (i_em_f.Count == 0)
                                {
                                    DirectoryInfo di_destino = new DirectoryInfo(ruta_destino + "\\" + dir_c_f.Name);

                                    if (di_destino.Exists == true)
                                    {
                                    }
                                    else
                                    {
                                        di_destino.Create();
                                    }

                                    File.Copy(f_c_f.FullName, di_destino + "\\" + f_c_f.Name, true);
                                    id_em = Guid.NewGuid();

                                    FFProbe ffProbe = new FFProbe();
                                    var videoInfo = ffProbe.GetMediaInfo(di_destino + "\\" + f_c_f.Name.ToString());
                                    string f_d = videoInfo.Duration.Hours + ":" + videoInfo.Duration.Minutes + ":" + videoInfo.Duration.Seconds;

                                    var g_media = new inf_exp_mat
                                    {
                                        id_exp_mat = id_em,
                                        ruta_archivo = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_wmv, ext_mp4),
                                        ruta_ext = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_wmv, ".pdf"),
                                        duracion = f_d,
                                        nom_archivo = f_c_f.Name.Replace(ext_wmv, ""),
                                        id_est_mat = 1,
                                        id_control_exp = id_ctrl,
                                        fecha_registro = DateTime.Now,
                                    };
                                    edm_master.inf_exp_mat.Add(g_media);
                                    edm_master.SaveChanges();

                                    try
                                    {
                                        FFMpegConverter ffMpegConverter = new FFMpegConverter();
                                        ffMpegConverter.ConvertMedia(di_destino + "\\" + f_c_f.Name, di_destino + "\\" + f_c_f.Name.Replace(ext_wmv, ext_mp4), Format.mp4);
                                        File.Delete(di_destino + "\\" + f_c_f.Name);
                                        est_matID = 2;

                                        var a_media = (from c in edm_master.inf_exp_mat
                                                       where c.id_exp_mat == id_em
                                                       select c).FirstOrDefault();

                                        a_media.id_est_mat = est_matID;
                                        edm_master.SaveChanges();
                                    }
                                    catch
                                    {
                                        est_matID = 3;

                                        var a_media = (from c in edm_master.inf_exp_mat
                                                       where c.id_exp_mat == id_em
                                                       select c).FirstOrDefault();

                                        a_media.id_est_mat = est_matID;
                                        edm_master.SaveChanges();
                                    }
                                }
                            }

                        }

                    }
                    else
                    {
                        string e_f = ruta_destino.Name.ToString();
                        var i_em_ff = (from c in edm_master.inf_master_jvl
                                       where c.sesion == e_f
                                       select c).ToList();

                        if (i_em_ff.Count == 0)
                        { }
                        else
                        {
                            id_ctrl = i_em_ff[0].id_control_exp;
                            foreach (FileInfo f_c_f in dir_c_f.GetFiles("*wmv"))
                            {
                                string f_f = ruta_destino + "\\" + dir_c_f.Name + "\\" + f_c_f.Name.Replace(ext_wmv, ext_mp4);
                                var i_em_f = (from c in edm_master.inf_exp_mat
                                              where c.ruta_archivo == f_f
                                              select c).ToList();

                                if (i_em_f.Count == 0)
                                {
                                    DirectoryInfo di_destino = new DirectoryInfo(ruta_destino + "\\" + dir_c_f.Name);

                                    if (di_destino.Exists == true)
                                    {
                                    }
                                    else
                                    {
                                        di_destino.Create();
                                    }

                                    File.Copy(f_c_f.FullName, di_destino + "\\" + f_c_f.Name, true);
                                    id_em = Guid.NewGuid();

                                    FFProbe ffProbe = new FFProbe();
                                    var videoInfo = ffProbe.GetMediaInfo(di_destino + "\\" + f_c_f.Name.ToString());
                                    string f_d = videoInfo.Duration.Hours + ":" + videoInfo.Duration.Minutes + ":" + videoInfo.Duration.Seconds;

                                    var g_media = new inf_exp_mat
                                    {
                                        id_exp_mat = id_em,
                                        ruta_archivo = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_wmv, ext_mp4),
                                        ruta_ext = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_wmv, ".pdf"),
                                        duracion = f_d,
                                        nom_archivo = f_c_f.Name.Replace(ext_wmv, ""),
                                        id_est_mat = 1,
                                        id_control_exp = id_ctrl,
                                        fecha_registro = DateTime.Now,
                                    };
                                    edm_master.inf_exp_mat.Add(g_media);
                                    edm_master.SaveChanges();

                                    try
                                    {
                                        FFMpegConverter ffMpegConverter = new FFMpegConverter();
                                        ffMpegConverter.ConvertMedia(di_destino + "\\" + f_c_f.Name, di_destino + "\\" + f_c_f.Name.Replace(ext_wmv, ext_mp4), Format.mp4);
                                        File.Delete(di_destino + "\\" + f_c_f.Name);
                                        est_matID = 2;

                                        var a_media = (from c in edm_master.inf_exp_mat
                                                       where c.id_exp_mat == id_em
                                                       select c).FirstOrDefault();

                                        a_media.id_est_mat = est_matID;
                                        edm_master.SaveChanges();
                                    }
                                    catch
                                    {
                                        est_matID = 3;

                                        var a_media = (from c in edm_master.inf_exp_mat
                                                       where c.id_exp_mat == id_em
                                                       select c).FirstOrDefault();

                                        a_media.id_est_mat = est_matID;
                                        edm_master.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var lis_asf = dir_c_f.GetFiles("*asf");
            if (lis_asf.Length > 0)
            {
                using (var edm_master = new bd_tsEntities())
                {
                    var i_master = (from c in edm_master.inf_master_jvl
                                    where c.sesion == dir_c_f.Name
                                    select c).ToList();

                    if (t_ss == 1)
                    {
                        if (i_master.Count == 0)
                        {
                            inf_master_jvl infMaster = new inf_master_jvl()
                            {
                                id_control_exp = id_ctrl,
                                sesion = dir_c_f.Name,
                                titulo = dir_c_f.Name,
                                err_carga = "Ninguno",
                                id_estatus_exp = 1,
                                id_estatus_qa = 1,
                                id_ruta_videos = id_rv,
                                fecha_registro = DateTime.Now
                            };
                            edm_master.inf_master_jvl.Add(infMaster);
                            edm_master.SaveChanges();

                            foreach (FileInfo f_c_f in dir_c_f.GetFiles("*asf"))
                            {
                                DirectoryInfo di_destino = new DirectoryInfo(ruta_destino + "\\" + dir_c_f.Name);

                                if (di_destino.Exists == true)
                                {
                                }
                                else
                                {
                                    di_destino.Create();
                                }

                                File.Copy(f_c_f.FullName, di_destino + "\\" + f_c_f.Name, true);
                                id_em = Guid.NewGuid();

                                FFProbe ffProbe = new FFProbe();
                                var videoInfo = ffProbe.GetMediaInfo(di_destino + "\\" + f_c_f.Name.ToString());
                                string f_d = videoInfo.Duration.Hours + ":" + videoInfo.Duration.Minutes + ":" + videoInfo.Duration.Seconds;

                                var g_media = new inf_exp_mat
                                {
                                    id_exp_mat = id_em,
                                    ruta_archivo = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_asf, ext_mp4),
                                    ruta_ext = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_asf, ".pdf"),
                                    duracion = f_d,
                                    nom_archivo = f_c_f.Name.Replace(ext_asf, ""),
                                    id_est_mat = 1,
                                    id_control_exp = id_ctrl,
                                    fecha_registro = DateTime.Now,
                                };
                                edm_master.inf_exp_mat.Add(g_media);
                                edm_master.SaveChanges();

                                try
                                {
                                    FFMpegConverter ffMpegConverter = new FFMpegConverter();
                                    ffMpegConverter.ConvertMedia(di_destino + "\\" + f_c_f.Name, di_destino + "\\" + f_c_f.Name.Replace(ext_asf, ext_mp4), Format.mp4);
                                    File.Delete(di_destino + "\\" + f_c_f.Name);
                                    est_matID = 2;

                                    var a_media = (from c in edm_master.inf_exp_mat
                                                   where c.id_exp_mat == id_em
                                                   select c).FirstOrDefault();

                                    a_media.id_est_mat = est_matID;
                                    edm_master.SaveChanges();
                                }
                                catch
                                {
                                    est_matID = 3;

                                    var a_media = (from c in edm_master.inf_exp_mat
                                                   where c.id_exp_mat == id_em
                                                   select c).FirstOrDefault();

                                    a_media.id_est_mat = est_matID;
                                    edm_master.SaveChanges();
                                }
                            }
                        }
                        else
                        {

                            id_ctrl = i_master[0].id_control_exp;
                            foreach (FileInfo f_c_f in dir_c_f.GetFiles("*asf"))
                            {
                                string f_f = ruta_destino + "\\" + dir_c_f.Name + "\\" + f_c_f.Name.Replace(ext_asf, ext_mp4);
                                var i_em_f = (from c in edm_master.inf_exp_mat
                                              where c.ruta_archivo == f_f
                                              select c).ToList();

                                if (i_em_f.Count == 0)
                                {
                                    DirectoryInfo di_destino = new DirectoryInfo(ruta_destino + "\\" + dir_c_f.Name);

                                    if (di_destino.Exists == true)
                                    {
                                    }
                                    else
                                    {
                                        di_destino.Create();
                                    }

                                    File.Copy(f_c_f.FullName, di_destino + "\\" + f_c_f.Name, true);
                                    id_em = Guid.NewGuid();

                                    FFProbe ffProbe = new FFProbe();
                                    var videoInfo = ffProbe.GetMediaInfo(di_destino + "\\" + f_c_f.Name.ToString());
                                    string f_d = videoInfo.Duration.Hours + ":" + videoInfo.Duration.Minutes + ":" + videoInfo.Duration.Seconds;

                                    var g_media = new inf_exp_mat
                                    {
                                        id_exp_mat = id_em,
                                        ruta_archivo = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_asf, ext_mp4),
                                        ruta_ext = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_asf, ".pdf"),
                                        duracion = f_d,
                                        nom_archivo = f_c_f.Name.Replace(ext_asf, ""),
                                        id_est_mat = 1,
                                        id_control_exp = id_ctrl,
                                        fecha_registro = DateTime.Now,
                                    };
                                    edm_master.inf_exp_mat.Add(g_media);
                                    edm_master.SaveChanges();

                                    try
                                    {
                                        FFMpegConverter ffMpegConverter = new FFMpegConverter();
                                        ffMpegConverter.ConvertMedia(di_destino + "\\" + f_c_f.Name, di_destino + "\\" + f_c_f.Name.Replace(ext_asf, ext_mp4), Format.mp4);
                                        File.Delete(di_destino + "\\" + f_c_f.Name);
                                        est_matID = 2;

                                        var a_media = (from c in edm_master.inf_exp_mat
                                                       where c.id_exp_mat == id_em
                                                       select c).FirstOrDefault();

                                        a_media.id_est_mat = est_matID;
                                        edm_master.SaveChanges();
                                    }
                                    catch
                                    {
                                        est_matID = 3;

                                        var a_media = (from c in edm_master.inf_exp_mat
                                                       where c.id_exp_mat == id_em
                                                       select c).FirstOrDefault();

                                        a_media.id_est_mat = est_matID;
                                        edm_master.SaveChanges();
                                    }
                                }
                            }

                        }

                    }
                    else
                    {
                        string e_f = ruta_destino.Name.ToString();
                        var i_em_ff = (from c in edm_master.inf_master_jvl
                                       where c.sesion == e_f
                                       select c).ToList();

                        if (i_em_ff.Count == 0)
                        { }
                        else
                        {
                            id_ctrl = i_em_ff[0].id_control_exp;
                            foreach (FileInfo f_c_f in dir_c_f.GetFiles("*asf"))
                            {
                                string f_f = ruta_destino + "\\" + dir_c_f.Name + "\\" + f_c_f.Name.Replace(ext_asf, ext_mp4);
                                var i_em_f = (from c in edm_master.inf_exp_mat
                                              where c.ruta_archivo == f_f
                                              select c).ToList();

                                if (i_em_f.Count == 0)
                                {
                                    DirectoryInfo di_destino = new DirectoryInfo(ruta_destino + "\\" + dir_c_f.Name);

                                    if (di_destino.Exists == true)
                                    {
                                    }
                                    else
                                    {
                                        di_destino.Create();
                                    }

                                    File.Copy(f_c_f.FullName, di_destino + "\\" + f_c_f.Name, true);
                                    id_em = Guid.NewGuid();

                                    FFProbe ffProbe = new FFProbe();
                                    var videoInfo = ffProbe.GetMediaInfo(di_destino + "\\" + f_c_f.Name.ToString());
                                    string f_d = videoInfo.Duration.Hours + ":" + videoInfo.Duration.Minutes + ":" + videoInfo.Duration.Seconds;

                                    var g_media = new inf_exp_mat
                                    {
                                        id_exp_mat = id_em,
                                        ruta_archivo = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_asf, ext_mp4),
                                        ruta_ext = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_asf, ".pdf"),
                                        duracion = f_d,
                                        nom_archivo = f_c_f.Name.Replace(ext_asf, ""),
                                        id_est_mat = 1,
                                        id_control_exp = id_ctrl,
                                        fecha_registro = DateTime.Now,
                                    };
                                    edm_master.inf_exp_mat.Add(g_media);
                                    edm_master.SaveChanges();

                                    try
                                    {
                                        FFMpegConverter ffMpegConverter = new FFMpegConverter();
                                        ffMpegConverter.ConvertMedia(di_destino + "\\" + f_c_f.Name, di_destino + "\\" + f_c_f.Name.Replace(ext_asf, ext_mp4), Format.mp4);
                                        File.Delete(di_destino + "\\" + f_c_f.Name);
                                        est_matID = 2;

                                        var a_media = (from c in edm_master.inf_exp_mat
                                                       where c.id_exp_mat == id_em
                                                       select c).FirstOrDefault();

                                        a_media.id_est_mat = est_matID;
                                        edm_master.SaveChanges();
                                    }
                                    catch
                                    {
                                        est_matID = 3;

                                        var a_media = (from c in edm_master.inf_exp_mat
                                                       where c.id_exp_mat == id_em
                                                       select c).FirstOrDefault();

                                        a_media.id_est_mat = est_matID;
                                        edm_master.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var lis_mp4 = dir_c_f.GetFiles("*mp4");
            if (lis_mp4.Length > 0)
            {

                using (var edm_master = new bd_tsEntities())
                {
                    var i_master = (from c in edm_master.inf_master_jvl
                                    where c.sesion == dir_c_f.Name
                                    select c).ToList();

                    if (t_ss == 1)
                    {
                        if (i_master.Count == 0)
                        {
                            inf_master_jvl infMaster = new inf_master_jvl()
                            {
                                id_control_exp = id_ctrl,
                                sesion = dir_c_f.Name,
                                titulo = dir_c_f.Name,
                                err_carga = "Ninguno",
                                id_estatus_exp = 1,
                                id_estatus_qa = 1,
                                id_ruta_videos = id_rv,
                                fecha_registro = DateTime.Now
                            };
                            edm_master.inf_master_jvl.Add(infMaster);
                            edm_master.SaveChanges();
                            foreach (FileInfo f_c_f in dir_c_f.GetFiles("*mp4"))
                            {
                            
                                    string f_f = ruta_destino + "\\" + dir_c_f.Name + "\\" + f_c_f.Name;
                                    var i_em_f = (from c in edm_master.inf_exp_mat
                                                  where c.ruta_archivo == f_f
                                                  select c).ToList();

                                    if (i_em_f.Count == 0)
                                    {
                                        DirectoryInfo di_destino = new DirectoryInfo(ruta_destino + "\\" + dir_c_f.Name);
                                        di_destino.Create();

                                        File.Copy(f_c_f.FullName, di_destino + "\\" + f_c_f.Name);

                                        var i_masterf = (from c in edm_master.inf_master_jvl
                                                        where c.id_control_exp == id_ctrl
                                                        select c).ToList();

                                        FFProbe ffProbe = new FFProbe();
                                        var videoInfo = ffProbe.GetMediaInfo(di_destino + "\\" + f_c_f.Name.ToString());
                                        string f_d = videoInfo.Duration.Hours + ":" + videoInfo.Duration.Minutes + ":" + videoInfo.Duration.Seconds;

                                        var g_media = new inf_exp_mat
                                        {
                                            id_exp_mat = Guid.NewGuid(),
                                            ruta_archivo = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_wmv, ext_mp4),
                                            ruta_ext = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_wmv, ".pdf"),
                                            duracion = f_d,
                                            nom_archivo = f_c_f.Name.Replace(ext_mp4, ""),
                                            id_est_mat = 2,
                                            id_control_exp = id_ctrl,
                                            fecha_registro = DateTime.Now,
                                        };

                                        edm_master.inf_exp_mat.Add(g_media);
                                        edm_master.SaveChanges();
                                    }
                                
                            }
                        }
                        else
                        {
                            foreach (FileInfo f_c_f in dir_c_f.GetFiles("*mp4"))
                            {

                                string f_f = ruta_destino + "\\" + dir_c_f.Name + "\\" + f_c_f.Name;
                                var i_em_f = (from c in edm_master.inf_exp_mat
                                              where c.ruta_archivo == f_f
                                              select c).ToList();

                                if (i_em_f.Count == 0)
                                {
                                    DirectoryInfo di_destino = new DirectoryInfo(ruta_destino + "\\" + dir_c_f.Name);
                                    di_destino.Create();

                                    File.Copy(f_c_f.FullName, di_destino + "\\" + f_c_f.Name);

                                    var i_masterf = (from c in edm_master.inf_master_jvl
                                                     where c.id_control_exp == id_ctrl
                                                     select c).ToList();

                                    FFProbe ffProbe = new FFProbe();
                                    var videoInfo = ffProbe.GetMediaInfo(di_destino + "\\" + f_c_f.Name.ToString());
                                    string f_d = videoInfo.Duration.Hours + ":" + videoInfo.Duration.Minutes + ":" + videoInfo.Duration.Seconds;

                                    var g_media = new inf_exp_mat
                                    {
                                        id_exp_mat = Guid.NewGuid(),
                                        ruta_archivo = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_wmv, ext_mp4),
                                        ruta_ext = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_wmv, ".pdf"),
                                        duracion = f_d,
                                        nom_archivo = f_c_f.Name.Replace(ext_mp4, ""),
                                        id_est_mat = 2,
                                        id_control_exp = id_ctrl,
                                        fecha_registro = DateTime.Now,
                                    };

                                    edm_master.inf_exp_mat.Add(g_media);
                                    edm_master.SaveChanges();
                                }

                            }
                        }
                    }
                    else
                    {
                        foreach (FileInfo f_c_f in dir_c_f.GetFiles("*mp4"))
                        {
                          
                                string f_f = ruta_destino + "\\" + dir_c_f.Name + "\\" + f_c_f.Name;
                                var i_em_f = (from c in edm_master.inf_exp_mat
                                              where c.ruta_archivo == f_f
                                              select c).ToList();

                                if (i_em_f.Count == 0)
                                {
                                    DirectoryInfo di_destino = new DirectoryInfo(ruta_destino + "\\" + dir_c_f.Name);
                                    di_destino.Create();

                                    File.Copy(f_c_f.FullName, di_destino + "\\" + f_c_f.Name);

                                    var i_masterf = (from c in edm_master.inf_master_jvl
                                                    where c.id_control_exp == id_ctrl
                                                    select c).ToList();

                                    FFProbe ffProbe = new FFProbe();
                                    var videoInfo = ffProbe.GetMediaInfo(di_destino + "\\" + f_c_f.Name.ToString());
                                    string f_d = videoInfo.Duration.Hours + ":" + videoInfo.Duration.Minutes + ":" + videoInfo.Duration.Seconds;

                                    var g_media = new inf_exp_mat
                                    {
                                        id_exp_mat = Guid.NewGuid(),
                                        ruta_archivo = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_wmv, ext_mp4),
                                        ruta_ext = di_destino + "\\" + f_c_f.Name.ToString().Replace(ext_wmv, ".pdf"),
                                        duracion = f_d,
                                        nom_archivo = f_c_f.Name.Replace(ext_mp4, ""),
                                        id_est_mat = 2,
                                        id_control_exp = id_ctrl,
                                        fecha_registro = DateTime.Now,
                                    };

                                    edm_master.inf_exp_mat.Add(g_media);
                                    edm_master.SaveChanges();
                                }
                            
                        }
                    }
                }
            }
        }
    }
}